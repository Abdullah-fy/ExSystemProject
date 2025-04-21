using ExSystemProject.Models;
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

        // Call stored procedure to create a course
       
        public void CreateCourse(Course course)
        {
            var crsNameParam = new SqlParameter("@crs_name", course.CrsName);
            var crsPeriodParam = new SqlParameter("@crs_period", course.CrsPeriod ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", course.InsId ?? (object)DBNull.Value);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateCourse @crs_name, @crs_period, @ins_id",
                crsNameParam, crsPeriodParam, insIdParam);
        }

        // Call stored procedure to update a course
        public void UpdateCourse(Course course)
        {
            // If isactive is null, get the current value from the database
            if (!course.Isactive.HasValue)
            {
                var currentCourse = _context.Courses
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CrsId == course.CrsId);

                course.Isactive = currentCourse?.Isactive ?? true;  // Default to true if somehow null
            }

            var crsIdParam = new SqlParameter("@crs_id", course.CrsId);
            var crsNameParam = new SqlParameter("@crs_name", course.CrsName);
            var crsPeriodParam = new SqlParameter("@crs_period", course.CrsPeriod ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", course.InsId ?? (object)DBNull.Value);

            // Always send an explicit value for isactive, never null
            var isActiveParam = new SqlParameter("@isactive", course.Isactive.Value);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateCourse @crs_id, @crs_name, @crs_period, @ins_id, @isactive",
                crsIdParam, crsNameParam, crsPeriodParam, insIdParam, isActiveParam);
        }


        // Call stored procedure to delete a course
        public void DeleteCourse(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteCourse @CrsId",
                crsIdParam);
        }

        // Call stored procedure to get course by id
        public Course GetCourseById(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            return _context.Courses
                .FromSqlRaw("EXEC sp_GetCourseById @CrsId", crsIdParam) 
                .AsEnumerable()
                .FirstOrDefault();
        }

        // Call stored procedure to get all courses
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

        // Call stored procedure to get course topics
        public List<Topic> GetCourseTopics(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            return _context.Topics
                .FromSqlRaw("EXEC sp_GetCourseTopics @CrsId", crsIdParam)
                .AsEnumerable()
                .ToList();
        }

        // Call stored procedure to get exams by course id
        public List<Exam> GetExamsByCourseId(int courseId)
        {
            var crsIdParam = new SqlParameter("@CrsId", courseId);

            return _context.Exams
                .FromSqlRaw("EXEC sp_GetExamsBy_crsid @CrsId", crsIdParam)
                .AsEnumerable()
                .ToList();
        }
    }
}
