using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class BranchRepo: GenaricRepo<Branch>
    {
        private readonly ExSystemTestContext context;

        public BranchRepo(ExSystemTestContext context): base(context)
        {
            this.context = context;
        }
        public List<Branch> GetAllActive()
        {
            return context.Branches.Where(a => a.Isactive == true).ToList();
        }
        public bool Exists(int id)
        {
            return context.Branches.Any(e => e.BranchId == id);
        }
    }
}
