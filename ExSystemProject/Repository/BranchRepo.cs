using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class BranchRepo : GenaricRepo<Branch>
    {
        private readonly ExSystemTestContext context;

        public BranchRepo(ExSystemTestContext context) : base(context)
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

        public List<Track> GetTracksByBranchId(int id)
        {
            return context.Tracks
             .FromSqlRaw("EXEC sp_GetTracksByBranchId @BranchId", new SqlParameter("@BranchId", id))
             .ToList();
        }

    }
}