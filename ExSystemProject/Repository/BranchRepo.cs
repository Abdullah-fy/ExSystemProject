using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class BranchRepo: GenaricRepo<Branch>
    {
        public BranchRepo(ExSystemTestContext context): base(context)
        {
            
        }
    }
}
