using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class QuestionRepo:GenaricRepo<Question>
    {
        public QuestionRepo(ExSystemTestContext context): base(context) 
        {
            
        }
    }
}
