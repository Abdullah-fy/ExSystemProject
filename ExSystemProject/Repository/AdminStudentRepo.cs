using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;

namespace ExSystemProject.Repository
{
    public class AdminStudentRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminStudentRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Get all students with optional filter for active/inactive
        public List<Student> GetAllStudents(bool? activeStudents = null)
        {
            var activeParam = activeStudents.HasValue
                ? new SqlParameter("@activeOnly", activeStudents.Value)
                : new SqlParameter("@activeOnly", DBNull.Value);

            return _context.Students
                .FromSqlRaw("EXEC sp_GetAllStudents @activeOnly", activeParam)
                .Include(s => s.User)
                .Include(s => s.Track)
                .ToList();
        }

        // Get student by ID
        public Student GetStudentById(int studentId)
        {
            var studentIdParam = new SqlParameter("@StudentID", studentId);

            return _context.Students
                .FromSqlRaw("EXEC sp_GetStudentById @StudentID", studentIdParam)
                .Include(s => s.User)
                .AsEnumerable()
                .FirstOrDefault();
        }

        // Get student by ID with branch information
        public Student GetStudentByIdWithBranch(int studentId)
        {
            var student = _context.Students
                .Include(s => s.User)
                .Include(s => s.Track)
                    .ThenInclude(t => t.Branch)
                .FirstOrDefault(s => s.StudentId == studentId);

            return student;
        }

        // Get students by track ID with optional filter for active/inactive
        public List<Student> GetStudentsByTrackId(int trackId, bool? activeStudents = true)
        {
            var trackIdParam = new SqlParameter("@TrackID", trackId);
            var activeParam = activeStudents.HasValue
                ? new SqlParameter("@ActiveOnly", activeStudents.Value)
                : new SqlParameter("@ActiveOnly", DBNull.Value);

            return _context.Students
                .FromSqlRaw("EXEC sp_GetStudentsByTrackId @TrackID, @ActiveOnly",
                    trackIdParam, activeParam)
                .Include(s => s.User)
                .ToList();
        }

        // Get students by branch ID with optional filter for active/inactive
        public List<Student> GetStudentsByBranchId(int branchId, bool? activeStudents = true)
        {
            var branchIdParam = new SqlParameter("@BranchID", branchId);
            var activeParam = activeStudents.HasValue
                ? new SqlParameter("@ActiveOnly", activeStudents.Value)
                : new SqlParameter("@ActiveOnly", DBNull.Value);

            return _context.Students
                .FromSqlRaw("EXEC sp_GetStudentsByBranchId @BranchID, @ActiveOnly",
                    branchIdParam, activeParam)
                .Include(s => s.User)
                .Include(s => s.Track)
                .ToList();
        }

        // Create a new student
        public Student CreateStudent(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
            return student;
        }

        // Update a student
        public void UpdateStudent(int studentId, string username, string email, string gender, int? trackId, bool isActive)
        {
            var studentIdParam = new SqlParameter("@StudentId", studentId);
            var usernameParam = new SqlParameter("@Username", username ?? (object)DBNull.Value);
            var emailParam = new SqlParameter("@Email", email ?? (object)DBNull.Value);
            var genderParam = new SqlParameter("@Gender", gender ?? (object)DBNull.Value);
            var trackIdParam = new SqlParameter("@TrackId", trackId.HasValue ? (object)trackId.Value : DBNull.Value);
            var isActiveParam = new SqlParameter("@IsActive", isActive);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateStudent @StudentId, @Username, @Email, @Gender, @TrackId, @IsActive",
                studentIdParam, usernameParam, emailParam, genderParam, trackIdParam, isActiveParam);
        }

        // Delete a student
        public void DeleteStudent(int studentId)
        {
            var studentIdParam = new SqlParameter("@StudentId", studentId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteStudent @StudentId",
                studentIdParam);
        }

        // Assign an exam to a student
        public void AssignExamToStudent(int examId, int studentId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);
            var studentIdParam = new SqlParameter("@StudentId", studentId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_AssignExamToStudent @ExamId, @StudentId",
                examIdParam, studentIdParam);
        }

        // Create a student using stored procedure
        public void CreateStudentWithStoredProcedure(string username, string email, string gender, string password, int? trackId)
        {
            var usernameParam = new SqlParameter("@Username", username ?? (object)DBNull.Value);
            var emailParam = new SqlParameter("@Email", email ?? (object)DBNull.Value);
            var genderParam = new SqlParameter("@Gender", gender ?? (object)DBNull.Value);
            var passwordParam = new SqlParameter("@Password", password ?? (object)DBNull.Value);
            var trackIdParam = new SqlParameter("@TrackId", trackId.HasValue ? (object)trackId.Value : DBNull.Value);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateStudent @Username, @Email, @Gender, @Password, @TrackId",
                usernameParam, emailParam, genderParam, passwordParam, trackIdParam);
        }
    }
}
