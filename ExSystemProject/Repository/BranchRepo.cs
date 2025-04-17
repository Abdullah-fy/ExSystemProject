using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class BranchRepo : GenaricRepo<Branch>
    {
        ExSystemTestContext _context;
        public BranchRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        public List<Track> GetTracksByBranchId(int id){
            return _context.Tracks
             .FromSqlRaw("EXEC sp_GetTracksByBranchId @BranchId", new SqlParameter("@BranchId", id))
             .ToList();
        }
    }
}
