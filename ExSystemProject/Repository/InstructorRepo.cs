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
            try
            {
                var parameter = new SqlParameter("@activeInstructor", SqlDbType.Bit)
                {
                    Value = activeInstructors ?? (object)DBNull.Value
                };

                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetAllInstructors @activeInstructor";
                    command.Parameters.Add(parameter);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();
                    var instructors = new List<Instructor>();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = result["Ins_Id"] != DBNull.Value ? (int)result["Ins_Id"] : 0,
                                Salary = result["Salary"] != DBNull.Value ? (decimal)result["Salary"] : 0,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int?)result["track_id"] : null,
                                UserId = result["userId"] != DBNull.Value ? (int?)result["userId"] : null,
                                User = new User
                                {
                                    UserId = result["userId"] != DBNull.Value ? (int)result["userId"] : 0,
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString()
                                },
                                Track = result["track_id"] != DBNull.Value ? new Track
                                {
                                    TrackId = (int)result["track_id"],
                                    TrackName = result["track_name"]?.ToString()
                                } : null
                            };

                            instructors.Add(instructor);
                        }
                    }

                    return instructors;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllInstructors: {ex.Message}");
                return new List<Instructor>();
            }
        }

        public Instructor GetInstructorById(int instructorId)
        {
            try
            {
                var parameter = new SqlParameter("@ins_id", SqlDbType.Int)
                {
                    Value = instructorId
                };

                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetInstructorById @ins_id";
                    command.Parameters.Add(parameter);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();
                    Instructor instructor = null;

                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                        {
                            instructor = new Instructor
                            {
                                InsId = result["Ins_Id"] != DBNull.Value ? (int)result["Ins_Id"] : 0,
                                Salary = result["Salary"] != DBNull.Value ? (decimal)result["Salary"] : 0,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int?)result["track_id"] : null,
                                UserId = result["userId"] != DBNull.Value ? (int?)result["userId"] : null,
                                User = new User
                                {
                                    UserId = result["userId"] != DBNull.Value ? (int)result["userId"] : 0,
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString(),
                                    Img = result["img"]?.ToString()
                                },
                                Track = result["track_id"] != DBNull.Value ? new Track
                                {
                                    TrackId = (int)result["track_id"],
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int)result["branch_id"] : 0
                                } : null
                            };

                            if (instructor.Track != null && result["branch_id"] != DBNull.Value)
                            {
                                instructor.Track.Branch = new Branch
                                {
                                    BranchId = (int)result["branch_id"],
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }
                        }
                    }

                    // If we found the instructor, manually load related courses
                    if (instructor != null)
                    {
                        instructor.Courses = GetInstructorCourses(instructorId);
                    }

                    return instructor;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInstructorById: {ex.Message}");
                return null;
            }
        }

        public List<Instructor> GetInstructorsByTrackId(int trackId, bool? getActive = true)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId },
                    new SqlParameter("@Getactive", SqlDbType.Bit) { Value = getActive ?? (object)DBNull.Value }
                };

                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetInstructorsByTrackId @track_id, @Getactive";
                    command.Parameters.AddRange(parameters);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();
                    var instructors = new List<Instructor>();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = result["Ins_Id"] != DBNull.Value ? (int)result["Ins_Id"] : 0,
                                Salary = result["Salary"] != DBNull.Value ? (decimal)result["Salary"] : 0,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int?)result["track_id"] : null,
                                UserId = result["userId"] != DBNull.Value ? (int?)result["userId"] : null,
                                User = new User
                                {
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString()
                                },
                                Track = result["track_id"] != DBNull.Value ? new Track
                                {
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int?)result["branch_id"] : null
                                } : null
                            };

                            if (instructor.Track != null && result["branch_name"] != DBNull.Value)
                            {
                                instructor.Track.Branch = new Branch
                                {
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }

                            instructors.Add(instructor);
                        }
                    }

                    return instructors;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInstructorsByTrackId: {ex.Message}");
                return new List<Instructor>();
            }
        }

        public List<Instructor> GetInstructorsByBranchId(int branchId, bool? getActive = true)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@branch_id", SqlDbType.Int) { Value = branchId },
                    new SqlParameter("@GetActive", SqlDbType.Bit) { Value = getActive ?? (object)DBNull.Value }
                };

                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetInstructorsByBranchId @branch_id, @GetActive";
                    command.Parameters.AddRange(parameters);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();
                    var instructors = new List<Instructor>();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = result["Ins_Id"] != DBNull.Value ? (int)result["Ins_Id"] : 0,
                                Salary = result["Salary"] != DBNull.Value ? (decimal)result["Salary"] : 0,
                                Isactive = result["isactive"] != DBNull.Value ? (bool)result["isactive"] : false,
                                TrackId = result["track_id"] != DBNull.Value ? (int?)result["track_id"] : null,
                                UserId = result["userId"] != DBNull.Value ? (int?)result["userId"] : null,
                                User = new User
                                {
                                    Username = result["username"]?.ToString(),
                                    Email = result["email"]?.ToString(),
                                    Gender = result["gender"]?.ToString()
                                },
                                Track = result["track_id"] != DBNull.Value ? new Track
                                {
                                    TrackName = result["track_name"]?.ToString(),
                                    BranchId = result["branch_id"] != DBNull.Value ? (int?)result["branch_id"] : null
                                } : null
                            };

                            if (instructor.Track != null && result["branch_name"] != DBNull.Value)
                            {
                                instructor.Track.Branch = new Branch
                                {
                                    BranchName = result["branch_name"]?.ToString()
                                };
                            }

                            instructors.Add(instructor);
                        }
                    }

                    return instructors;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInstructorsByBranchId: {ex.Message}");
                return new List<Instructor>();
            }
        }

        public void CreateInstructor(string username, string email, string gender, string password, decimal salary, int trackId)
        {
            try
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

                // Ensure connection is open
                if (_context.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    _context.Database.OpenConnection();
                }

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_CreateInstructor @username, @email, @gender, @password, @salary, @track_id",
                    parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateInstructor: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        public void UpdateInstructor(int insId, string username, string email, string gender, decimal salary, int trackId, bool isActive)
        {
            try
            {
                Console.WriteLine($"Updating instructor {insId} with username={username}, email={email}, active={isActive}");

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

                // Ensure connection is open
                if (_context.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    _context.Database.OpenConnection();
                }

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_UpdateInstructor @ins_id, @username, @email, @gender, @salary, @track_id, @isactive",
                    parameters);

                Console.WriteLine("Instructor updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateInstructor: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        public void DeleteInstructor(int insId)
        {
            try
            {
                var parameter = new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId };

                // Ensure connection is open
                if (_context.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    _context.Database.OpenConnection();
                }

                _context.Database.ExecuteSqlRaw("EXEC sp_DeleteInstructor @ins_id", parameter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteInstructor: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        public List<Course> GetInstructorCourses(int insId, bool? active = true)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId },
                    new SqlParameter("@active", SqlDbType.Bit) { Value = active ?? true }
                };

                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetInstructorCourses @ins_id, @active";
                    command.Parameters.AddRange(parameters);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();
                    var courses = new List<Course>();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            var course = new Course
                            {
                                CrsId = result["CrsId"] != DBNull.Value ? (int)result["CrsId"] : 0,
                                CrsName = result["CrsName"]?.ToString(),
                                CrsPeriod = result["CrsPeriod"] != DBNull.Value ? (int?)result["CrsPeriod"] : null,
                                InsId = result["ins_id"] != DBNull.Value ? (int?)result["ins_id"] : null,
                                Isactive = result["isactive"] != DBNull.Value ? (bool?)result["isactive"] : null
                            };

                            courses.Add(course);
                        }
                    }

                    // Load related collections for each course
                    foreach (var course in courses)
                    {
                        // Load student courses
                        course.StudentCourses = _context.StudentCourses
                            .Where(sc => sc.CrsId == course.CrsId)
                            .ToList();

                        // Load exams
                        course.Exams = _context.Exams
                            .Where(e => e.CrsId == course.CrsId)
                            .ToList();
                    }

                    return courses;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInstructorCourses: {ex.Message}");
                return new List<Course>();
            }
        }

        public dynamic GetInstructorCoursesWithStudentCount(int insId)
        {
            try
            {
                var parameter = new SqlParameter("@InstructorId", SqlDbType.Int) { Value = insId };

                // Use direct SQL execution to avoid EF Core tracking issues
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC sp_GetInstructorCoursesWithStudentCount @InstructorId";
                    command.Parameters.Add(parameter);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();
                    var coursesReport = new List<dynamic>();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            coursesReport.Add(new
                            {
                                CourseId = (int)result["CrsId"],
                                CourseName = result["CrsName"]?.ToString(),
                                StudentCount = (int)result["StudentCount"],
                                PassedStudents = (int)result["PassedStudents"],
                                AverageGrade = result["AverageGrade"] != DBNull.Value ? (decimal)result["AverageGrade"] : 0,
                                CourseStatus = ((bool)result["isactive"]) ? "Active" : "Inactive"
                            });
                        }
                    }

                    return coursesReport;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInstructorCoursesWithStudentCount: {ex.Message}");
                return new List<dynamic>();
            }
        }
    }
}
