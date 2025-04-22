using ExSystemProject.Models;
using ExSystemProject.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
﻿using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;

namespace ExSystemProject.Repository
{
    public class StudentRepo : GenaricRepo<Student>
    {
<<<<<<< HEAD
        ExSystemTestContext _context;

        public StudentRepo(ExSystemTestContext context ) : base(context)
        {
            this._context = context;
         }
        public Student getByUserId(int userId)
        {
            var result = _context.Students.FirstOrDefault(i => i.UserId == userId && i.Isactive == true);
            return result;
        }


        public void AddNewStudent(StudentViewModel model)
        {
            _context.Database.ExecuteSqlRaw(
            "EXEC sp_CreateStudent @p0, @p1, @p2, @p3, @p4, @p5",
            model.username, model.Email, model.Gender, model.password, model.TrackId, model.Image);
        }

        public List<Student> GetStudentByInstructorAndCourse(int instructorId, int courseId)
        {
            var students = _context.Students
                .FromSqlRaw("EXEC GetStudentsByCourseAndInstructor @CourseId, @InstructorId",
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@InstructorId", instructorId))
                .ToList();
=======
        private readonly ExSystemTestContext _context;

        public StudentRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        public List<Student> GetAllStudents(bool? activeStudents = null)
        {
            var parameter = new SqlParameter("@activeStudent", SqlDbType.Bit)
            {
                Value = activeStudents ?? (object)DBNull.Value
            };

            var students = new List<Student>();

            try
            {
                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetAllStudentsWithBranch @activeStudent";
                    command.Parameters.Add(parameter);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var student = new Student
                            {
                                StudentId = result["StudentId"] != DBNull.Value ? (int)result["StudentId"] : 0,
                                EnrollmentDate = result["EnrollmentDate"] != DBNull.Value ? (DateTime)result["EnrollmentDate"] : null,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int)result["track_id"] : null,
                                User = new User
                                {
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString()
                                },
                                Track = new Track
                                {
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int)result["branch_id"] : 0
                                }
                            };

                            if (result["branch_name"] != DBNull.Value)
                            {
                                student.Track.Branch = new Branch
                                {
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }

                            students.Add(student);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllStudents: {ex.Message}");
            }
>>>>>>> d9b930721eb08b0249c6985ba249afaa6e9e0b81

            return students;
        }

<<<<<<< HEAD
          
=======
        public Student GetStudentById(int studentId)
        {
            var parameter = new SqlParameter("@student_id", SqlDbType.Int)
            {
                Value = studentId
            };

            // First get the student from the stored procedure
            var student = _context.Students
                .FromSqlRaw("EXEC sp_GetStudentById @student_id", parameter)
                .AsEnumerable() // Convert to in-memory enumerable before composing
                .FirstOrDefault();

            // If we found a student, load the related data separately
            if (student != null)
            {
                // Manually load the related entities
                _context.Entry(student).Reference(s => s.User).Load();
                _context.Entry(student).Reference(s => s.Track).Load();

                // Load student courses
                _context.Entry(student).Collection(s => s.StudentCourses).Load();
                foreach (var course in student.StudentCourses)
                {
                    _context.Entry(course).Reference(c => c.Crs).Load();
                }

                // Load student exams
                _context.Entry(student).Collection(s => s.StudentExams).Load();
                foreach (var exam in student.StudentExams)
                {
                    _context.Entry(exam).Reference(e => e.Exam).Load();
                }
            }

            return student;
        }

        public Student GetStudentByIdWithBranch(int studentId)
        {
            var parameter = new SqlParameter("@student_id", SqlDbType.Int)
            {
                Value = studentId
            };

            var student = new Student();

            try
            {
                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetStudentByIdWithBranch @student_id";
                    command.Parameters.Add(parameter);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();

                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                        {
                            student = new Student
                            {
                                StudentId = result["StudentId"] != DBNull.Value ? (int)result["StudentId"] : 0,
                                EnrollmentDate = result["EnrollmentDate"] != DBNull.Value ? (DateTime)result["EnrollmentDate"] : null,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int)result["track_id"] : null,
                                UserId = result["userId"] != DBNull.Value ? (int)result["userId"] : 0,
                                User = new User
                                {
                                    UserId = result["userId"] != DBNull.Value ? (int)result["userId"] : 0,
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString(),
                                    Img = result["img"]?.ToString()
                                },
                                Track = new Track
                                {
                                    TrackId = result["track_id"] != DBNull.Value ? (int)result["track_id"] : 0,
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int)result["branch_id"] : 0
                                }
                            };

                            if (result["branch_id"] != DBNull.Value)
                            {
                                student.Track.Branch = new Branch
                                {
                                    BranchId = (int)result["branch_id"],
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }

                            // Load student courses
                            _context.Entry(student).Collection(s => s.StudentCourses).Load();
                            foreach (var course in student.StudentCourses)
                            {
                                _context.Entry(course).Reference(c => c.Crs).Load();
                            }

                            // Load student exams
                            _context.Entry(student).Collection(s => s.StudentExams).Load();
                            foreach (var exam in student.StudentExams)
                            {
                                _context.Entry(exam).Reference(e => e.Exam).Load();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentByIdWithBranch: {ex.Message}");
            }

            return student;
        }

        public List<Student> GetStudentsByTrackId(int trackId, bool? activeStudents = true)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId },
                    new SqlParameter("@activeStudent", SqlDbType.Bit) { Value = activeStudents ?? true }
                };

                // Execute using raw SQL to avoid composition issues
                var students = new List<Student>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetStudentsByDepartmentWithBranch @track_id, @activeStudent";
                    command.Parameters.Add(parameters[0]);
                    command.Parameters.Add(parameters[1]);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var student = new Student
                            {
                                StudentId = result["StudentId"] != DBNull.Value ? (int)result["StudentId"] : 0,
                                EnrollmentDate = result["EnrollmentDate"] != DBNull.Value ? (DateTime)result["EnrollmentDate"] : null,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int)result["track_id"] : null,
                                User = new User
                                {
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString()
                                },
                                Track = new Track
                                {
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int)result["branch_id"] : 0
                                }
                            };

                            if (result["branch_name"] != DBNull.Value)
                            {
                                student.Track.Branch = new Branch
                                {
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }

                            students.Add(student);
                        }
                    }
                }

                return students;
            }
            catch (Exception ex)
            {
                // Log the exception if possible
                Console.WriteLine($"Error in GetStudentsByTrackId: {ex.Message}");
                return new List<Student>();
            }
        }

        public List<Student> GetStudentsByBranchId(int branchId, bool? activeStudents = true)
        {
            try
            {
                // First check if branch exists
                var branch = _context.Branches.Find(branchId);
                if (branch == null)
                    return new List<Student>();

                // Use direct SQL query with parameters to get results
                var parameters = new[]
                {
                    new SqlParameter("@branch_id", SqlDbType.Int) { Value = branchId },
                    new SqlParameter("@ActiveOnly", SqlDbType.Bit) { Value = activeStudents ?? true }
                };

                // Execute stored procedure and map results
                var students = new List<Student>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetStudentsByBranchIdWithBranch @branch_id, @ActiveOnly";
                    command.Parameters.Add(parameters[0]);
                    command.Parameters.Add(parameters[1]);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var student = new Student
                            {
                                StudentId = result["StudentId"] != DBNull.Value ? (int)result["StudentId"] : 0,
                                EnrollmentDate = result["EnrollmentDate"] != DBNull.Value ? (DateTime)result["EnrollmentDate"] : null,
                                Isactive = result["IsActive"] != DBNull.Value ? (bool)result["IsActive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int)result["track_id"] : null,
                                User = new User
                                {
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString()
                                },
                                Track = new Track
                                {
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int)result["branch_id"] : 0
                                }
                            };

                            if (result["branch_name"] != DBNull.Value)
                            {
                                student.Track.Branch = new Branch
                                {
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }

                            students.Add(student);
                        }
                    }
                }

                return students;
            }
            catch (Exception ex)
            {
                // Log the exception if possible
                Console.WriteLine($"Error in GetStudentsByBranchId: {ex.Message}");
                return new List<Student>();
            }
        }

        public Student CreateStudent(Student student)
        {
            var parameters = new[]
            {
                new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = student.User?.Username },
                new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = student.User?.Email },
                new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = student.User?.Gender },
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = student.User?.Upassword },
                new SqlParameter("@track_id", SqlDbType.Int) { Value = (object)student.TrackId ?? DBNull.Value }
            };

            var result = _context.Students
                .FromSqlRaw("EXEC sp_CreateStudent @username, @email, @gender, @password, @track_id", parameters)
                .AsEnumerable()
                .FirstOrDefault();

            return result;
        }

        public void UpdateStudent(int studentId, string username, string email, string gender, int? trackId, bool isActive)
        {
            try
            {
                Console.WriteLine($"UpdateStudent called with: studentId={studentId}, username={username}, email={email}, gender={gender}, trackId={trackId}, isActive={isActive}");

                var parameters = new[]
                {
                    new SqlParameter("@student_id", SqlDbType.Int) { Value = studentId },
                    new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = username },
                    new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = email },
                    new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = gender },
                    new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId.HasValue ? (object)trackId.Value : DBNull.Value },
                    new SqlParameter("@isactive", SqlDbType.Bit) { Value = isActive }
                };

                // Ensure the connection is open
                if (_context.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    _context.Database.OpenConnection();
                }

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_UpdateStudent @student_id, @username, @email, @gender, @track_id, @isactive",
                    parameters);

                Console.WriteLine("UpdateStudent completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStudent: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw; // Rethrow to allow caller to handle
            }
        }

        public void DeleteStudent(int studentId)
        {
            var parameter = new SqlParameter("@student_id", SqlDbType.Int)
            {
                Value = studentId
            };

            _context.Database.ExecuteSqlRaw("EXEC sp_DeleteStudent @student_id", parameter);
        }

        public void AssignExamToStudent(int examId, int studentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@exam_id", SqlDbType.Int) { Value = examId },
                new SqlParameter("@student_id", SqlDbType.Int) { Value = studentId }
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_AssignExamToStudent @exam_id, @student_id",
                parameters);
        }

        public void CreateStudentWithStoredProcedure(string username, string email, string gender, string password, int? trackId)
        {
            try
            {
                Console.WriteLine($"Creating student with Username={username}, Email={email}, Gender={gender}, TrackId={trackId}");

                var parameters = new[]
                {
                    new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = username },
                    new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = email },
                    new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = gender },
                    new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = password },
                    new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId.HasValue ? (object)trackId.Value : DBNull.Value }
                };

                // Explicitly ensure connection is open
                if (_context.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    _context.Database.OpenConnection();
                }

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_CreateStudent @username, @email, @gender, @password, @track_id",
                    parameters);

                Console.WriteLine("Student created successfully");
            }
            catch (Exception ex)
            {
                // Enhanced error logging
                Console.WriteLine($"Error in CreateStudentWithStoredProcedure: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow to allow caller to handle
            }
        }



        public Student GetStudentByIdBETA(int studentId)
        {
            Student student = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetStudentById";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@student_id", SqlDbType.Int) { Value = studentId });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            student = new Student
                            {
                                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                TrackId = reader.IsDBNull(reader.GetOrdinal("track_id")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("track_id")),
                                UserId = reader.GetInt32(reader.GetOrdinal("userId")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                User = new User
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Gender = reader.GetString(reader.GetOrdinal("gender")),
                                    Img = reader.IsDBNull(reader.GetOrdinal("img")) ? null : reader.GetString(reader.GetOrdinal("img"))
                                },
                                Track = reader.IsDBNull(reader.GetOrdinal("track_name")) ? null : new Track
                                {
                                    TrackName = reader.GetString(reader.GetOrdinal("track_name"))
                                }
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving student by ID: {ex.Message}");
                    throw;
                }
            }

            return student;
        }


        public Student Getstd(int userid)
        {
            var stdd = _context.Students.FirstOrDefault(s => s.UserId == userid);
            return stdd; 
        }


        // repo to insert into student-course 

        public bool Enrollment(int userid , int crsid)
        {
            var student = _context.Students.FirstOrDefault(s => s.UserId == userid);
          //  var course = _context.Courses.FirstOrDefault(c => c.CrsId == crsid);

            var exists = _context.StudentCourses.Any(s => s.StudentId == student.StudentId && s.CrsId == crsid);

            if (!exists) {

                var stdcrs = new StudentCourse()
                {
                    CrsId = crsid,
                    StudentId = student.StudentId
                };

                _context.StudentCourses.Add(stdcrs);
                _context.SaveChanges();
                return true; 

            }
            else
            {
                return false;
            }





        }

        public bool ISEnroll(int userid , int crsid)
        {
            var student = _context.Students.FirstOrDefault(s => s.UserId == userid);
           bool dd =  _context.StudentCourses.Any(s => s.StudentId == student.StudentId && s.CrsId == crsid);

            if (!dd)
            {
                return true; 
            }
            return false; 
        }

>>>>>>> d9b930721eb08b0249c6985ba249afaa6e9e0b81
    }

   


}
