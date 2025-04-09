using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class UserAssignmentRepo:GenaricRepo<UserAssignment>
    {
        public UserAssignmentRepo(ExSystemTestContext context):base(context)
        {
            
        }
    }
}
