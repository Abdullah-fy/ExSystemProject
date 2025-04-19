using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;

namespace ExSystemProject.Repository
{
    public class InstructorRepo : GenaricRepo<Instructor>
    {
        private readonly ExSystemTestContext _context;

        public InstructorRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

       
        public List<InstructorDTO> getAllWithUserData()
        {
            List<Instructor> instructors = _context.Instructors.Include(i => i.User).ToList();
            List<InstructorDTO> insDto = new List<InstructorDTO>();

            foreach (var item in instructors)
            {
                insDto.Add(new InstructorDTO
                {
                    InsId = item.InsId,
                    Username = item.User.Username,
                    UserId = item.User.UserId
                });

            }
            return insDto;
        }

       
        public List<Instructor> GetAllInstructors(bool? activeInstructors = true)
        {
            var parameter = new SqlParameter("@activeInstructor", SqlDbType.Bit)
            {
                Value = activeInstructors
            };

            var instructors = _context.Instructors
                .FromSqlRaw("EXEC sp_GetAllInstructors @activeInstructor", parameter)
                .Include(i => i.User)
                .Include(i => i.Track)
                .ToList();

            return instructors;
        }

        public Instructor GetInstructorById(int instructorId)
        {
            var parameter = new SqlParameter("@ins_id", SqlDbType.Int)
            {
                Value = instructorId
            };

            var instructor = _context.Instructors
                .FromSqlRaw("EXEC sp_GetInstructorById @ins_id", parameter)
                .Include(i => i.User)
                .Include(i => i.Track)
                .AsEnumerable()
                .FirstOrDefault();

            return instructor;
        }

        public List<Instructor> GetInstructorsByTrackId(int trackId, bool? getActive = true)
        {
            var parameters = new[]
            {
                new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId },
                new SqlParameter("@Getactive", SqlDbType.Bit) { Value = getActive }
            };

            var instructors = _context.Instructors
                .FromSqlRaw("EXEC sp_GetInstructorsByTrackId @track_id, @Getactive", parameters)
                .Include(i => i.User)
                .Include(i => i.Track)
                .ToList();

            return instructors;
        }

        public List<Instructor> GetInstructorsByBranchId(int branchId, bool? getActive = true)
        {
            var parameters = new[]
            {
                new SqlParameter("@branch_id", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@GetActive", SqlDbType.Bit) { Value = getActive }
            };

            // This is a more complex query that may return a different structure
            var results = _context.Database
                .SqlQueryRaw<dynamic>("EXEC sp_GetInstructorsByBranchId @branch_id, @GetActive", parameters)
                .ToList();

            // Convert dynamic results to Instructor objects
            var instructors = new List<Instructor>();
            foreach (var result in results)
            {
                var instructor = new Instructor
                {
                    InsId = result.Ins_Id,
                    Salary = result.Salary,
                    Isactive = result.IsActive,
                    User = new User
                    {
                        Username = result.username,
                        Email = result.email,
                        Gender = result.gender
                    },
                    Track = new Track
                    {
                        TrackName = result.track_name
                    }
                };

                instructors.Add(instructor);
            }

            return instructors;
        }

        public void CreateInstructor(string username, string email, string gender, string password, decimal salary, int trackId)
        {
            var parameters = new[]
            {
                new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = username },
                new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = email },
                new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = gender },
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = password },
                new SqlParameter("@salary", SqlDbType.Decimal) { Value = salary, Precision = 10, Scale = 2 },
                new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId }
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateInstructor @username, @email, @gender, @password, @salary, @track_id",
                parameters);
        }

        public void UpdateInstructor(int insId, string username, string email, string gender, decimal salary, int trackId, bool isActive)
        {
            var parameters = new[]
            {
                new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId },
                new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = username },
                new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = email },
                new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = gender },
                new SqlParameter("@salary", SqlDbType.Decimal) { Value = salary, Precision = 10, Scale = 2 },
                new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId },
                new SqlParameter("@isactive", SqlDbType.Bit) { Value = isActive }
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateInstructor @ins_id, @username, @email, @gender, @salary, @track_id, @isactive",
                parameters);
        }

        public void DeleteInstructor(int insId)
        {
            var parameter = new SqlParameter("@ins_id", SqlDbType.Int)
            {
                Value = insId
            };

            _context.Database.ExecuteSqlRaw("EXEC sp_DeleteInstructor @ins_id", parameter);
        }

        public List<Course> GetInstructorCourses(int insId, bool? active = true)
        {
            var parameters = new[]
            {
                new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId },
                new SqlParameter("@active", SqlDbType.Bit) { Value = active }
            };

            var courses = _context.Courses
                .FromSqlRaw("EXEC sp_GetInstructorCourses @ins_id, @active", parameters)
                .ToList();

            return courses;
        }

        public dynamic GetInstructorCoursesWithStudentCount(int insId)
        {
            var parameter = new SqlParameter("@InstructorId", SqlDbType.Int)
            {
                Value = insId
            };

            var report = _context.Database
                .SqlQueryRaw<dynamic>("EXEC sp_GetInstructorCoursesWithStudentCount @InstructorId", parameter)
                .ToList();

            return report;
        }
    }
}
