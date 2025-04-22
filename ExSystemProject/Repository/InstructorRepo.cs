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

        // Get all instructors with branch information using stored procedure
        public List<Instructor> GetAllInstructorsWithBranch(bool? activeInstructors = true)
        {
            var instructors = new List<Instructor>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetAllInstructorsWithBranch";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@activeInstructor", SqlDbType.Bit) { Value = activeInstructors ?? (object)DBNull.Value });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = reader.GetInt32(reader.GetOrdinal("InsId")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                TrackId = reader.IsDBNull(reader.GetOrdinal("track_id")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("track_id")),
                                UserId = reader.GetInt32(reader.GetOrdinal("userId")),
                                User = new User
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Gender = reader.GetString(reader.GetOrdinal("gender")),
                                    Img = reader.IsDBNull(reader.GetOrdinal("img")) ? null : reader.GetString(reader.GetOrdinal("img"))
                                },
                                Track = reader.IsDBNull(reader.GetOrdinal("track_id")) ? null : new Track
                                {
                                    TrackId = reader.GetInt32(reader.GetOrdinal("TrackId")),
                                    TrackName = reader.GetString(reader.GetOrdinal("track_name")),
                                    BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                    Branch = new Branch
                                    {
                                        BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                        BranchName = reader.GetString(reader.GetOrdinal("branch_name"))
                                    }
                                }
                            };

                            instructors.Add(instructor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving instructors: {ex.Message}");
                    throw;
                }
            }

            return instructors;
        }

        // Get instructor by ID with branch information
        public Instructor GetInstructorByIdWithBranch(int instructorId)
        {
            Instructor instructor = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetInstructorByIdWithBranch";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@instructor_id", SqlDbType.Int) { Value = instructorId });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            instructor = new Instructor
                            {
                                InsId = reader.GetInt32(reader.GetOrdinal("InsId")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                TrackId = reader.IsDBNull(reader.GetOrdinal("track_id")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("track_id")),
                                UserId = reader.GetInt32(reader.GetOrdinal("userId")),
                                User = new User
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Gender = reader.GetString(reader.GetOrdinal("gender")),
                                    Img = reader.IsDBNull(reader.GetOrdinal("img")) ? null : reader.GetString(reader.GetOrdinal("img"))
                                },
                                Track = reader.IsDBNull(reader.GetOrdinal("track_id")) ? null : new Track
                                {
                                    TrackId = reader.GetInt32(reader.GetOrdinal("track_id")),
                                    TrackName = reader.GetString(reader.GetOrdinal("track_name")),
                                    BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                    Branch = new Branch
                                    {
                                        BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                        BranchName = reader.GetString(reader.GetOrdinal("branch_name"))
                                    }
                                }
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving instructor with ID {instructorId}: {ex.Message}");
                    throw;
                }
            }

            return instructor;
        }

        // Get instructors by track with branch information
        public List<Instructor> GetInstructorsByTrackWithBranch(int trackId, bool? getActive = true)
        {
            var instructors = new List<Instructor>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetInstructorsByTrackWithBranch";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId });
                command.Parameters.Add(new SqlParameter("@activeInstructor", SqlDbType.Bit) { Value = getActive ?? (object)DBNull.Value });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = reader.GetInt32(reader.GetOrdinal("InsId")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                TrackId = reader.GetInt32(reader.GetOrdinal("track_id")),
                                UserId = reader.GetInt32(reader.GetOrdinal("userId")),
                                User = new User
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Gender = reader.GetString(reader.GetOrdinal("gender"))
                                },
                                Track = new Track
                                {
                                    TrackId = reader.GetInt32(reader.GetOrdinal("track_id")),
                                    TrackName = reader.GetString(reader.GetOrdinal("track_name")),
                                    BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                    Branch = new Branch
                                    {
                                        BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                        BranchName = reader.GetString(reader.GetOrdinal("branch_name"))
                                    }
                                }
                            };

                            instructors.Add(instructor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving instructors by track ID {trackId}: {ex.Message}");
                    throw;
                }
            }

            return instructors;
        }

        // Get instructors by branch with branch information
        public List<Instructor> GetInstructorsByBranchWithBranch(int branchId, bool? getActive = true)
        {
            var instructors = new List<Instructor>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetInstructorsByBranchWithBranch";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@branch_id", SqlDbType.Int) { Value = branchId });
                command.Parameters.Add(new SqlParameter("@activeInstructor", SqlDbType.Bit) { Value = getActive ?? (object)DBNull.Value });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = reader.GetInt32(reader.GetOrdinal("InsId")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                TrackId = reader.GetInt32(reader.GetOrdinal("track_id")),
                                UserId = reader.GetInt32(reader.GetOrdinal("userId")),
                                User = new User
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Gender = reader.GetString(reader.GetOrdinal("gender"))
                                },
                                Track = new Track
                                {
                                    TrackId = reader.GetInt32(reader.GetOrdinal("track_id")),
                                    TrackName = reader.GetString(reader.GetOrdinal("track_name")),
                                    BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                    Branch = new Branch
                                    {
                                        BranchId = reader.GetInt32(reader.GetOrdinal("branch_id")),
                                        BranchName = reader.GetString(reader.GetOrdinal("branch_name"))
                                    }
                                }
                            };

                            instructors.Add(instructor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving instructors by branch ID {branchId}: {ex.Message}");
                    throw;
                }
            }

            return instructors;
        }

        // Create new instructor using stored procedure
        public void CreateInstructor(string username, string email, string gender, string password, decimal salary, int trackId)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_CreateInstructor";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = username });
                command.Parameters.Add(new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = email });
                command.Parameters.Add(new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = gender });
                command.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = password });
                command.Parameters.Add(new SqlParameter("@salary", SqlDbType.Decimal) { Value = salary, Precision = 10, Scale = 2 });
                command.Parameters.Add(new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId });

                try
                {
                    _context.Database.OpenConnection();
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Instructor created successfully with username: {username}, email: {email}, track: {trackId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating instructor: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
        }

        // Update instructor using stored procedure
        public void UpdateInstructor(int insId, string username, string email, string gender, decimal salary, int trackId, bool isActive)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_UpdateInstructor";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId });
                command.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar, 100) { Value = username });
                command.Parameters.Add(new SqlParameter("@email", SqlDbType.VarChar, 50) { Value = email });
                command.Parameters.Add(new SqlParameter("@gender", SqlDbType.NChar, 1) { Value = gender });
                command.Parameters.Add(new SqlParameter("@salary", SqlDbType.Decimal) { Value = salary, Precision = 10, Scale = 2 });
                command.Parameters.Add(new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId });
                command.Parameters.Add(new SqlParameter("@isactive", SqlDbType.Bit) { Value = isActive });

                try
                {
                    _context.Database.OpenConnection();
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Instructor with ID {insId} updated successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating instructor with ID {insId}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
        }

        // Delete instructor using stored procedure
        public void DeleteInstructor(int insId)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_DeleteInstructor";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId });

                try
                {
                    _context.Database.OpenConnection();
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Instructor with ID {insId} deleted successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting instructor with ID {insId}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
        }

        // Get instructor courses
        public List<Course> GetInstructorCourses(int insId, bool? active = true)
        {
            var courses = new List<Course>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetInstructorCourses";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ins_id", SqlDbType.Int) { Value = insId });
                command.Parameters.Add(new SqlParameter("@active", SqlDbType.Bit) { Value = active ?? (object)DBNull.Value });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var course = new Course
                            {
                                CrsId = reader.GetInt32(reader.GetOrdinal("Crs_Id")),
                                CrsName = reader.GetString(reader.GetOrdinal("Crs_Name")),
                                CrsPeriod = reader.IsDBNull(reader.GetOrdinal("Crs_period")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Crs_period")),
                                InsId = reader.IsDBNull(reader.GetOrdinal("ins_id")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("ins_id")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                description = reader.GetString(reader.GetOrdinal("Description")), 
                                Poster = reader.GetString(reader.GetOrdinal("Poster"))
                            };

                            courses.Add(course);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving courses for instructor with ID {insId}: {ex.Message}");
                    throw;
                }
            }

            return courses;
        }

        // Get instructor courses with student count report
        public dynamic GetInstructorCoursesWithStudentCount(int insId)
        {
            dynamic result = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetInstructorCoursesWithStudentCount";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@InstructorId", SqlDbType.Int) { Value = insId });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        var coursesList = new List<dynamic>();

                        while (reader.Read())
                        {
                            var course = new
                            {
                                CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                                CourseName = reader.GetString(reader.GetOrdinal("CourseName")),
                                CoursePeriod = reader.IsDBNull(reader.GetOrdinal("CoursePeriod")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("CoursePeriod")),
                                StudentCount = reader.GetInt32(reader.GetOrdinal("StudentCount")),
                                PassedStudents = reader.GetInt32(reader.GetOrdinal("PassedStudents")),
                                FailedStudents = reader.GetInt32(reader.GetOrdinal("FailedStudents")),
                                AverageGrade = reader.GetDouble(reader.GetOrdinal("AverageGrade")),
                                CourseStatus = reader.GetString(reader.GetOrdinal("CourseStatus"))
                            };

                            coursesList.Add(course);
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            var summary = new
                            {
                                InstructorName = reader.GetString(reader.GetOrdinal("InstructorName")),
                                InstructorId = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                TrackName = reader.IsDBNull(reader.GetOrdinal("TrackName")) ? null : reader.GetString(reader.GetOrdinal("TrackName")),
                                TotalCourses = reader.GetInt32(reader.GetOrdinal("TotalCourses")),
                                TotalStudents = reader.GetInt32(reader.GetOrdinal("TotalStudents")),
                                CourseStatusSummary = reader.GetString(reader.GetOrdinal("CourseStatusSummary"))
                            };

                            result = new
                            {
                                Courses = coursesList,
                                Summary = summary
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving course statistics for instructor with ID {insId}: {ex.Message}");
                    throw;
                }
            }

            return result;
        }


        // extend this repo to return all instructors by track id 
        public List<Instructor> GetInstructorsByTrackId(int trackId, bool? isActive = true)
        {
            var instructors = new List<Instructor>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_GetInstructorsByTrackId";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@track_id", SqlDbType.Int) { Value = trackId });
                command.Parameters.Add(new SqlParameter("@Getactive", SqlDbType.Bit)
                {
                    Value = isActive.HasValue ? (object)isActive.Value : DBNull.Value
                });

                try
                {
                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var instructor = new Instructor
                            {
                                InsId = reader.GetInt32(reader.GetOrdinal("Ins_Id")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Isactive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                                TrackId = reader.IsDBNull(reader.GetOrdinal("track_id")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("track_id")),
                                UserId = reader.GetInt32(reader.GetOrdinal("userId")),
                                User = new User
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Gender = reader.GetString(reader.GetOrdinal("gender")),
                                    // Optional: img or others if needed
                                }
                            };

                            instructors.Add(instructor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving instructors by track: {ex.Message}");
                    throw;
                }
            }

            return instructors;
        }

    }
}
