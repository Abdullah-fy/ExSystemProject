using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class ExamRepo:GenaricRepo<Exam>
    {
        public ExamRepo(ExSystemTestContext context): base(context) 
        {
            
        }
    }
}
