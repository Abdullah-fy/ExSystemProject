using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class StudentAnswerRepo:GenaricRepo<StudentAnswer>
    {
        public StudentAnswerRepo(ExSystemTestContext context):base(context) 
        {
            
        }
    }
}
