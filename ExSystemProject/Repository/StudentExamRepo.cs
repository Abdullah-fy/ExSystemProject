using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class StudentExamRepo:GenaricRepo<StudentExam>
    {
        public StudentExamRepo(ExSystemTestContext context):base(context) 
        {
            
        }
    }
}
