using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class StudentRepo:GenaricRepo<Student>
    {
        public StudentRepo(ExSystemTestContext constext):base(constext) 
        {
            
        }
    }
}
