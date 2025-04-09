using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class StudentCourseRepo:GenaricRepo<StudentCourse>
    {
        public StudentCourseRepo(ExSystemTestContext context):base(context)
        {
            
        }
    }
}
