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
        
    }
}
