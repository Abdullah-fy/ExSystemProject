using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Repository
{
    public class AdminBranchRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminBranchRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Get all active branches
        public List<Branch> GetAllActive()
        {
            return _context.Branches.Where(a => a.Isactive == true).ToList();
        }

        // Check if branch exists
        public bool Exists(int id)
        {
            return _context.Branches.Any(e => e.BranchId == id);
        }

        // Get all branches
        public List<Branch> GetAll()
        {
            return _context.Branches.ToList();
        }

        // Get tracks by branch ID
        public List<Track> GetTracksByBranchId(int id)
        {
            return _context.Tracks
                .FromSqlRaw("EXEC sp_GetTracksByBranchId @BranchId", new SqlParameter("@BranchId", id))
                .ToList();
        }

        // Create a branch
        public void CreateBranch(Branch branch)
        {
            _context.Branches.Add(branch);
            _context.SaveChanges();
        }

        // Update a branch
        public void UpdateBranch(Branch branch)
        {
            _context.Entry(branch).State = EntityState.Modified;
            _context.SaveChanges();
        }

        // Delete a branch
        public void DeleteBranch(int id)
        {
            var branch = _context.Branches.Find(id);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                _context.SaveChanges();
            }
        }

        // Get branch by ID
        public Branch GetBranchById(int id)
        {
            return _context.Branches.Find(id);
        }
    }
}
