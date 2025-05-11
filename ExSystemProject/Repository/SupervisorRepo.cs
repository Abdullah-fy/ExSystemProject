
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public List<UserAssignment> GetAllSupervisors(bool? activeOnly = true)
        {
            var query = _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Branch)
                .Include(ua => ua.Track)
                .Where(ua => ua.User.Role == "supervisor");

            if (activeOnly.HasValue)
            {
                query = query.Where(ua => ua.Isactive == activeOnly.Value);
            }

            return query.ToList();
        }

        public UserAssignment GetSupervisorById(int id)
        {
            return _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Branch)
                .Include(ua => ua.Track)
                .FirstOrDefault(ua => ua.AssignmentId == id && ua.User.Role == "supervisor");
        }

        public UserAssignment GetSupervisorByUserId(int userId)
        {
            return _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Branch)
                .Include(ua => ua.Track)
                .FirstOrDefault(ua => ua.UserId == userId && ua.User.Role == "supervisor");
        }

        public List<UserAssignment> GetSupervisorsByBranchId(int branchId, bool? activeOnly = null)
        {
            var query = _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Branch)
                .Include(ua => ua.Track)
                .Where(ua => ua.BranchId == branchId && ua.User.Role == "supervisor");

            if (activeOnly.HasValue)
            {
                query = query.Where(ua => ua.Isactive == activeOnly.Value);
            }

            return query.ToList();
        }

        public List<UserAssignment> GetSupervisorsByTrackId(int trackId, bool? activeOnly = null)
        {
            var query = _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Branch)
                .Include(ua => ua.Track)
                .Where(ua => ua.TrackId == trackId && ua.User.Role == "supervisor");

            if (activeOnly.HasValue)
            {
                query = query.Where(ua => ua.Isactive == activeOnly.Value);
            }

            return query.ToList();
        }

        public UserAssignment CreateSupervisor(int userId, int branchId, int? trackId = null)
        {
            try
            {
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
                    TrackId = trackId,
                    Isactive = true
                };

                _context.UserAssignments.Add(supervisor);
                _context.SaveChanges();

                return GetSupervisorById(supervisor.AssignmentId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateSupervisor: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; 
            }
        }

        public UserAssignment UpdateSupervisor(int assignmentId, int? branchId, int? trackId, bool isActive)
        {
            var assignment = _context.UserAssignments
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.AssignmentId == assignmentId && ua.User.Role == "supervisor");

            if (assignment != null)
            {
                if (branchId.HasValue)
                    assignment.BranchId = branchId;

                assignment.TrackId = trackId;
                assignment.Isactive = isActive;

                if (assignment.User != null)
                {
                    assignment.User.Isactive = isActive;
                }

                _context.SaveChanges();
                return GetSupervisorById(assignmentId);
            }

            return null;
        }

        public bool DeactivateSupervisor(int assignmentId)
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
                return true;
            }

            return false;
        }

        public int GetSupervisorCountByBranchAsync(int branchId)
        {
            return _context.UserAssignments
                .Include(ua => ua.User)
                .Count(s => s.BranchId == branchId && s.User.Role == "supervisor" && s.Isactive == true);
        }

        public List<Student> GetStudentsUnderSupervisor(int supervisorId)
        {
            var supervisor = GetSupervisorById(supervisorId);

            if (supervisor == null)
                return new List<Student>();

            if (supervisor.TrackId.HasValue)
            {
                return _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Track)
                    .Where(s => s.TrackId == supervisor.TrackId && s.Isactive == true)
                    .ToList();
            }
            else if (supervisor.BranchId.HasValue)
            {
                return _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Track)
                    .Where(s => s.Track.BranchId == supervisor.BranchId && s.Isactive == true)
                    .ToList();
            }

            return new List<Student>();
        }

        public List<Instructor> GetInstructorsUnderSupervisor(int supervisorId)
        {
            var supervisor = GetSupervisorById(supervisorId);

            if (supervisor == null)
                return new List<Instructor>();

            if (supervisor.TrackId.HasValue)
            {
                return _context.Instructors
                    .Include(i => i.User)
                    .Include(i => i.Track)
                    .Where(i => i.TrackId == supervisor.TrackId && i.Isactive == true)
                    .ToList();
            }
            else if (supervisor.BranchId.HasValue)
            {
                return _context.Instructors
                    .Include(i => i.User)
                    .Include(i => i.Track)
                    .Where(i => i.Track.BranchId == supervisor.BranchId && i.Isactive == true)
                    .ToList();
            }

            return new List<Instructor>();
        }

        public List<Course> GetCoursesUnderSupervisor(int supervisorId)
        {
            var instructors = GetInstructorsUnderSupervisor(supervisorId);

            if (instructors.Count == 0)
                return new List<Course>();

            var instructorIds = instructors.Select(i => i.InsId).ToList();

            return _context.Courses
                .Include(c => c.Ins)
                .Where(c => instructorIds.Contains(c.InsId ?? 0) && c.Isactive == true)
                .ToList();
        }

        public List<Exam> GetExamsUnderSupervisor(int supervisorId)
        {
            var courses = GetCoursesUnderSupervisor(supervisorId);

            if (courses.Count == 0)
                return new List<Exam>();

            var courseIds = courses.Select(c => c.CrsId).ToList();

            return _context.Exams
                .Include(e => e.Crs)
                .Include(e => e.Ins)
                .Where(e => courseIds.Contains(e.CrsId ?? 0) && e.Isactive == true)
                .ToList();
        }
    }
}
