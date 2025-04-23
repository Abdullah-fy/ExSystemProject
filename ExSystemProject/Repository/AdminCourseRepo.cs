using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExSystemProject.Repository
{
    public class AdminCourseRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminCourseRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        public void CreateCourse(Course course)
        {
            var crsNameParam = new SqlParameter("@crs_name", course.CrsName);
            var crsPeriodParam = new SqlParameter("@crs_period", course.CrsPeriod ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", course.InsId ?? (object)DBNull.Value);
            var descriptionParam = new SqlParameter("@description", string.IsNullOrEmpty(course.Description) ? (object)DBNull.Value : course.Description);
            var posterParam = new SqlParameter("@poster", string.IsNullOrEmpty(course.Poster) ? (object)DBNull.Value : course.Poster);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateCourse @crs_name, @crs_period, @ins_id, @poster, @description",
                crsNameParam, crsPeriodParam, insIdParam, posterParam, descriptionParam);
        }

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
            var isActiveParam = new SqlParameter("@isactive", course.Isactive.Value);
            var descriptionParam = new SqlParameter("@description", string.IsNullOrEmpty(course.Description) ? (object)DBNull.Value : course.Description);
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
    }
}
