using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class UserRepo:GenaricRepo<User>
    {
        public ExSystemTestContext context { get; }

        public UserRepo(ExSystemTestContext context) : base(context)
        {
            this.context = context;
        }
        public User getActiveByName(string name)
            {
            return context.Users.FirstOrDefault(a => a.Username == name && a.Isactive == true);
            }
        public User GetByEmail(string email)
        {
            return context.Users.FirstOrDefault(a => a.Email == email);
        }

    }
}
