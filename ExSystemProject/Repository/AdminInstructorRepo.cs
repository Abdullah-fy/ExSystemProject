using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Repository
{
    public class AdminInstructorRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminInstructorRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Get all instructors with user data
        public List<InstructorDTO> getAllWithUserData()
        {
            var instructors = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Track)
                .ToList();

            var instructorDTOs = new List<InstructorDTO>();
            foreach (var instructor in instructors)
            {
                instructorDTOs.Add(new InstructorDTO
                {
                    InsId = instructor.InsId,
                    UserId = instructor.UserId ?? 0,
                    Username = instructor.User?.Username,
                    Email = instructor.User?.Email,
                    Gender = instructor.User?.Gender,
                    TrackId = instructor.TrackId,
                    TrackName = instructor.Track?.TrackName,
                    Salary = instructor.Salary ?? 0,
                    Isactive = instructor.Isactive ?? true
                });
            }

            return instructorDTOs;
        }

        // Get all instructors with branch information
        public List<Instructor> GetAllInstructorsWithBranch(bool? activeInstructors = true)
        {
            var query = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Track)
                    .ThenInclude(t => t.Branch)
                .AsQueryable();

            if (activeInstructors.HasValue)
            {
                query = query.Where(i => i.Isactive == activeInstructors.Value);
            }

            return query.ToList();
        }

        // Get instructor by ID with branch information
        public Instructor GetInstructorByIdWithBranch(int instructorId)
        {
            return _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Track)
                    .ThenInclude(t => t.Branch)
                .FirstOrDefault(i => i.InsId == instructorId);
        }

        // Get instructors by track ID with branch information
        public List<Instructor> GetInstructorsByTrackWithBranch(int trackId, bool? getActive = true)
        {
            var query = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Track)
                    .ThenInclude(t => t.Branch)
                .Where(i => i.TrackId == trackId);

            if (getActive.HasValue)
            {
                query = query.Where(i => i.Isactive == getActive.Value);
            }

            return query.ToList();
        }

        // Get instructors by branch ID with branch information
        public List<Instructor> GetInstructorsByBranchWithBranch(int branchId, bool? getActive = true)
        {
            var query = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Track)
                    .ThenInclude(t => t.Branch)
                .Where(i => i.Track.BranchId == branchId);

            if (getActive.HasValue)
            {
                query = query.Where(i => i.Isactive == getActive.Value);
            }

            return query.ToList();
        }

        // Create a new instructor
        public void CreateInstructor(string username, string email, string gender, string password, decimal salary, int trackId)
        {
            var usernameParam = new SqlParameter("@Username", username);
            var emailParam = new SqlParameter("@Email", email);
            var genderParam = new SqlParameter("@Gender", gender);
            var passwordParam = new SqlParameter("@Password", password);
            var salaryParam = new SqlParameter("@Salary", salary);
            var trackIdParam = new SqlParameter("@TrackId", trackId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateInstructor @Username, @Email, @Gender, @Password, @Salary, @TrackId",
                usernameParam, emailParam, genderParam, passwordParam, salaryParam, trackIdParam);
        }

        // Update an instructor
        public void UpdateInstructor(int insId, string username, string email, string gender, decimal salary, int trackId, bool isActive)
        {
            var insIdParam = new SqlParameter("@InsId", insId);
            var usernameParam = new SqlParameter("@Username", username ?? (object)DBNull.Value);
            var emailParam = new SqlParameter("@Email", email ?? (object)DBNull.Value);
            var genderParam = new SqlParameter("@Gender", gender ?? (object)DBNull.Value);
            var salaryParam = new SqlParameter("@Salary", salary);
            var trackIdParam = new SqlParameter("@TrackId", trackId);
            var isActiveParam = new SqlParameter("@IsActive", isActive);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateInstructor @InsId, @Username, @Email, @Gender, @Salary, @TrackId, @IsActive",
                insIdParam, usernameParam, emailParam, genderParam, salaryParam, trackIdParam, isActiveParam);
        }

        // Delete an instructor
        public void DeleteInstructor(int insId)
        {
            var insIdParam = new SqlParameter("@InsId", insId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteInstructor @InsId",
                insIdParam);
        }

        // Get instructor courses
        public List<Course> GetInstructorCourses(int insId, bool? active = true)
        {
            var query = _context.Courses
                .Where(c => c.InsId == insId);

            if (active.HasValue)
            {
                query = query.Where(c => c.Isactive == active.Value);
            }

            return query.ToList();
        }

        // Get instructor courses with student count
        public dynamic GetInstructorCoursesWithStudentCount(int insId)
        {
            var courses = _context.Courses
                .Where(c => c.InsId == insId)
                .Select(c => new
                {
                    CourseId = c.CrsId,
                    CourseName = c.CrsName,
                    StudentCount = c.StudentCourses.Count
                })
                .ToList();

            return courses;
        }
    }
}
