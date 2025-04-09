using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class CourseRepo:GenaricRepo<Course>
    {
        public CourseRepo(ExSystemTestContext context): base(context)
        {
            
        }
    }
}
