using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class StudentCourseRepo:GenaricRepo<StudentCourse>
    {
        ExSystemTestContext _context;
        public StudentCourseRepo(ExSystemTestContext context):base(context)
        {
            _context = context;
        }

        public List<StudentCourse> GetStudentCourses(int crsId)
        {
            return _context.StudentCourses.Where(a => a.CrsId == crsId).ToList();
        }
        // Add to StudentCourseRepo class
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
                // Create a new enrollment (StudentCourse)
                var enrollment = new StudentCourse
                {
                    StudentId = studentId,
                    CrsId = courseId,
                    // Convert DateTime.Now to DateOnly
                    EnrolledAt = DateOnly.FromDateTime(DateTime.Now),
                    Isactive = true
                    // Grade and ispassed will be null by default
                };

                // Add to context
                _context.StudentCourses.Add(enrollment);

                // Save changes immediately
                _context.SaveChanges();

                Console.WriteLine($"Enrolled student {studentId} in course {courseId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enrolling student: {ex.Message}");
                throw; // Re-throw to be handled by the controller
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




    }
}
