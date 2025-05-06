using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExSystemProject.Repository
{
    public class StudentCourseRepo : GenaricRepo<StudentCourse>
    {
        ExSystemTestContext _context;
        public StudentCourseRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        public List<StudentCourse> GetStudentCourses(int crsId)
        {
            return _context.StudentCourses.Where(a => a.CrsId == crsId).ToList();
        }

        public bool IsStudentEnrolled(int studentId, int courseId)
        {
            return _context.StudentCourses.Any(sc =>
                sc.StudentId == studentId &&
                sc.CrsId == courseId &&
                sc.Isactive == true);
        }

        public void EnrollStudent(int studentId, int courseId)
        {
            try
            {
                var enrollment = new StudentCourse
                {
                    StudentId = studentId,
                    CrsId = courseId,
                    EnrolledAt = DateOnly.FromDateTime(DateTime.Now),
                    Isactive = true
                };

                _context.StudentCourses.Add(enrollment);
                _context.SaveChanges();

                Console.WriteLine($"Enrolled student {studentId} in course {courseId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enrolling student: {ex.Message}");
                throw;
            }
        }

        public void UnenrollStudent(int studentId, int courseId)
        {
            var enrollment = _context.StudentCourses
                .FirstOrDefault(sc => sc.StudentId == studentId && sc.CrsId == courseId);

            if (enrollment != null)
            {
                enrollment.Isactive = false;
                _context.SaveChanges();
            }
        }

        public List<StudentCourse> GetAllStudentCoursesByBranch(int branchId)
        {
            return _context.StudentCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Student.Track)
                .Where(sc => sc.Student.Track.BranchId == branchId && sc.Isactive == true)
                .ToList();
        }

        public List<StudentCourse> GetStudentCoursesByStudentId(int studentId)
        {
            return _context.StudentCourses
                .Include(sc => sc.Crs)
                .Where(sc => sc.StudentId == studentId && sc.Isactive == true)
                .ToList();
        }

        public List<StudentExam> GetStudentExamsByStudentId(int studentId)
        {
            return _context.StudentExams
                .Include(se => se.Exam)
                .Include(se => se.Student)
                .Where(se => se.StudentId == studentId && se.Isactive == true)
                .ToList();
        }

        internal dynamic GetByStudentId(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<List<AllStudentCoursesDTO>> GetStudentCoursesAsync(int studentId)
        {
            var courses = new List<AllStudentCoursesDTO>();

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "GetStudentCourses";
                command.CommandType = CommandType.StoredProcedure;

                var studentParam = command.CreateParameter();
                studentParam.ParameterName = "@student_id";
                studentParam.Value = studentId;
                studentParam.DbType = DbType.Int32;
                command.Parameters.Add(studentParam);

                await _context.Database.OpenConnectionAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    courses.Add(new AllStudentCoursesDTO
                    {
                        Crs_Id = reader.GetInt32(0),
                        Crs_Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Crs_period = reader.GetInt32(3),
                        EnrolledAt = reader.GetDateTime(4),
                        Grade = reader.IsDBNull(5) ? null : reader.GetString(5) // Handle NULL
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving student courses: {ex.Message}");
                throw;
            }
            finally
            {
                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }

            return courses;
        }

    }
}
