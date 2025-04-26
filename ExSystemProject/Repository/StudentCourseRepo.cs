using ExSystemProject.Models;

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
            var enrollment = new StudentCourse
            {
                StudentId = studentId,
                CrsId = courseId,
                Isactive = true,
                
                EnrolledAt = DateOnly.FromDateTime(DateTime.Now),
                Grade = null,
                Ispassed = null
            };

            _context.StudentCourses.Add(enrollment);
            _context.SaveChanges();
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



    }
}
