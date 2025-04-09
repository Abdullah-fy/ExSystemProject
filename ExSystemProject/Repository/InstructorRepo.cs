using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class InstructorRepo:GenaricRepo<Instructor>
    {
        public InstructorRepo(ExSystemTestContext context):base(context)
        {
            
        }
    }
}
