using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class ChoicesRepo: GenaricRepo<Choice>
    {
        public ChoicesRepo(ExSystemTestContext context):base(context)
        {
            
        }
    }
}
