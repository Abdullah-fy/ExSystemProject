// ExSystemProject/Repository/SupervisorRepo.cs
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Repository
{
    public class SupervisorRepo : GenaricRepo<UserAssignment>
    {
        private readonly ExSystemTestContext _context;

        public SupervisorRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        // Get all supervisors
        public List<UserAssignment> GetAllSupervisors(bool? activeOnly = true)
        {
            var query = _context.UserAssignments
                .Include(ua => ua.User)
                .Where(ua => ua.User.Role == "supervisor");

            if (activeOnly.HasValue)
            {
                query = query.Where(ua => ua.Isactive == activeOnly.Value);
            }

            return query.ToList();
        }

        // Get supervisor by ID
        public UserAssignment GetSupervisorById(int id)
        {
            return _context.UserAssignments
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.AssignmentId == id && ua.User.Role == "supervisor");
        }

        // Get supervisors by branch ID
        public List<UserAssignment> GetSupervisorsByBranchId(int branchId, bool? activeOnly = null)
        {
            var query = _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Track)
                .Where(ua => ua.BranchId == branchId && ua.User.Role == "supervisor");

            if (activeOnly.HasValue)
            {
                query = query.Where(ua => ua.Isactive == activeOnly.Value);
            }

            return query.ToList();
        }


        // Create a new supervisor
        public void CreateSupervisor(int userId, string username, string email, int branchId)
        {
            try
            {
                // First check if the user exists and has the supervisor role
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                if (user.Role != "supervisor")
                {
                    throw new InvalidOperationException("User is not a supervisor");
                }

                var supervisor = new UserAssignment
                {
                    UserId = userId,
                    BranchId = branchId,
                    Isactive = true
                };

                _context.UserAssignments.Add(supervisor);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateSupervisor: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw so the controller can handle it
            }
        }


        // Update supervisor details
        public void UpdateSupervisor(int assignmentId, int userId, string username, string email, int? branchId, bool isActive)
        {
            var assignment = _context.UserAssignments
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.AssignmentId == assignmentId && ua.User.Role == "supervisor");

            if (assignment != null)
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    // Update user information
                    user.Username = username;
                    user.Email = email;
                    user.Isactive = isActive;
                }

                // Update assignment
                assignment.BranchId = branchId;
                assignment.Isactive = isActive;

                _context.SaveChanges();
            }
        }

        // Deactivate supervisor
        public void DeactivateSupervisor(int assignmentId)
        {
            var assignment = _context.UserAssignments
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.AssignmentId == assignmentId && ua.User.Role == "supervisor");

            if (assignment != null)
            {
                assignment.Isactive = false;
                if (assignment.User != null)
                {
                    assignment.User.Isactive = false;
                }
                _context.SaveChanges();
            }
        }

        // Get supervisor count by branch
        public int GetSupervisorCountByBranchAsync(int branchId)
        {
            return _context.UserAssignments
                .Include(ua => ua.User)
                .Count(s => s.BranchId == branchId && s.User.Role == "supervisor" && s.Isactive == true);
        }
    }
}
