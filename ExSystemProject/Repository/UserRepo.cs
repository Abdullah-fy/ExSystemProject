using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class UserRepo:GenaricRepo<User>
    {
        public UserRepo(ExSystemTestContext context):base(context) 
        {
            
        }
    }
}
