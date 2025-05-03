using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
