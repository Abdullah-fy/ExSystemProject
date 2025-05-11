using ExSystemProject.Models;
using Humanizer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExSystemProject.Repository
{
    public class CourseRepo : GenaricRepo<Course>
    {
        private readonly ExSystemTestContext _context;

        public CourseRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<Course> GetAllCourses(bool? isActive = null, int? branchId = null, int? trackId = null)
        {
            var query = _context.Courses
                .Include(c => c.Ins)
                .ThenInclude(i => i.Track)
                .ThenInclude(t => t.Branch)
                .Include(c => c.Ins)
                .ThenInclude(i => i.User)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(c => c.Isactive == isActive.Value);
            if (branchId.HasValue)
                query = query.Where(c => c.Ins != null && c.Ins.Track != null && c.Ins.Track.BranchId == branchId.Value);
            if (trackId.HasValue)
                query = query.Where(c => c.Ins != null && c.Ins.TrackId == trackId.Value);

            return query;
        }

        public void CreateCourse(Course course)
        {
            var crsNameParam = new SqlParameter("@crs_name", course.CrsName);
            var crsPeriodParam = new SqlParameter("@crs_period", course.CrsPeriod ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", course.InsId ?? (object)DBNull.Value);
            var descriptionParam = new SqlParameter("@description", string.IsNullOrEmpty(course.description) ? (object)DBNull.Value : course.description);
            var posterParam = new SqlParameter("@poster", string.IsNullOrEmpty(course.Poster) ? (object)DBNull.Value : course.Poster);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateCourse @crs_name, @crs_period, @ins_id, @poster, @description",
                crsNameParam, crsPeriodParam, insIdParam, posterParam, descriptionParam);
        }

        public void UpdateCourse(Course course)
        {
            if (!course.Isactive.HasValue)
            {
                var currentCourse = _context.Courses
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CrsId == course.CrsId);

                course.Isactive = currentCourse?.Isactive ?? true;  
            }

            var crsIdParam = new SqlParameter("@crs_id", course.CrsId);
            var crsNameParam = new SqlParameter("@crs_name", course.CrsName);
            var crsPeriodParam = new SqlParameter("@crs_period", course.CrsPeriod ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", course.InsId ?? (object)DBNull.Value);
            var isActiveParam = new SqlParameter("@isactive", course.Isactive.Value);
            var descriptionParam = new SqlParameter("@description", string.IsNullOrEmpty(course.description) ? (object)DBNull.Value : course.description);
            var posterParam = new SqlParameter("@poster", string.IsNullOrEmpty(course.Poster) ? (object)DBNull.Value : course.Poster);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateCourse @crs_id, @crs_name, @crs_period, @ins_id, @isactive, @poster, @description",
                crsIdParam, crsNameParam, crsPeriodParam, insIdParam, isActiveParam, posterParam, descriptionParam);
        }


        public void DeleteCourse(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteCourse @CrsId",
                crsIdParam);
        }

        public Course GetCourseById(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            return _context.Courses
                .FromSqlRaw("EXEC sp_GetCourseById @CrsId", crsIdParam) 
                .AsEnumerable()
                .FirstOrDefault();
        }

        public List<Course> GetAllCourses(bool? activeOnly = null)
        {
            var activeParam = activeOnly.HasValue
                ? new SqlParameter("@activeCourse", activeOnly.Value)
                : new SqlParameter("@activeCourse", DBNull.Value);

            return _context.Courses
                .FromSqlRaw("EXEC sp_GetAllCourses @activeCourse", activeParam)
                .AsEnumerable()
                .ToList();
        }

        public List<Topic> GetCourseTopics(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            return _context.Topics
                .FromSqlRaw("EXEC sp_GetCourseTopics @CrsId", crsIdParam)
                .AsEnumerable()
                .ToList();
        }

        public List<Exam> GetExamsByCourseId(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            return _context.Exams
                .FromSqlRaw("EXEC sp_GetExamsBy_crsid @CrsId", crsIdParam)
                .AsEnumerable()
                .ToList();
        }


        public List<Course> GetCoursesByInstructor(int instructorId)
        {
            return _context.Courses.Where(a => a.InsId == instructorId).ToList();
        }


        public List<Course> getCourseByStudentAndInstructor(int studentId, int instructorId)
        {
            var  course = _context.Courses.FromSqlRaw("EXEC GetCourseByInstructorIdAndStudentId @instructorId, @studentId",
                new SqlParameter("@instructorId", instructorId),
                new SqlParameter("@studentId", studentId)).ToList();

            return course;
        }

        public List<Course> InstructorCourses(int instructorId)
        {
            return _context.Courses.Where(c => c.InsId == instructorId).ToList() ?? new List<Course>();
        }
        public int GetCourseCountByBranchAsync(int branchId)
        {
            return _context.Courses
                .Include(c => c.Ins)
                .Include(c => c.Ins.Track)
                .Where(c => c.Ins.Track.BranchId == branchId && c.Isactive == true)
                .Count();
        }
       

        public List<Course> GetCoursesByBranch(int branchId)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Ins)
                    .Include(c => c.Ins.Track)
                    .Include(c => c.Ins.User)
                    .Where(c => c.Ins.Track.BranchId == branchId && c.Isactive == true)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving courses for branch {branchId}: {ex.Message}", ex);
            }
        }

        public Course GetCourseWithBranch(int courseId)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Ins)
                    .Include(c => c.Ins.Track)
                    .Include(c => c.Ins.Track.Branch)
                    .Include(c => c.Ins.User)
                    .FirstOrDefault(c => c.CrsId == courseId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving course {courseId} with branch information: {ex.Message}", ex);
            }
        }
        public bool AssignCourseToInstructor(int courseId, int instructorId)
        {
            try
            {
                var courseIdParam = new SqlParameter("@CourseId", courseId);
                var instructorIdParam = new SqlParameter("@InstructorId", instructorId);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_AssignCourseToInstructor @CourseId, @InstructorId",
                    courseIdParam, instructorIdParam);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error assigning course: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }




    }
}
