﻿using ExSystemProject.Repository;
using Microsoft.EntityFrameworkCore;

public class UserAssignmentRepo : GenaricRepo<UserAssignment>
{
    private readonly ExSystemTestContext _context;

    public UserAssignmentRepo(ExSystemTestContext context) : base(context)
    {
        _context = context;
    }

    public List<UserAssignment> GetAllManagers(bool? activeOnly = true)
    {
        return _context.UserAssignments
            .Include(ua => ua.User)
            .Include(ua => ua.Branch)
            .Where(ua => ua.User.Role == "admin" &&
                  (activeOnly == null || ua.Isactive == activeOnly))
            .ToList();
    }

    public List<UserAssignment> GetManagersByBranchId(int branchId, bool? activeOnly = true)
    {
        return _context.UserAssignments
            .Include(ua => ua.User)
            .Include(ua => ua.Branch)
            .Where(ua => ua.User.Role == "admin" &&
                  ua.BranchId == branchId &&
                  (activeOnly == null || ua.Isactive == activeOnly))
            .ToList();
    }

    public UserAssignment GetManagerById(int id)
    {
        return _context.UserAssignments
            .Include(ua => ua.User)
            .Include(ua => ua.Branch)
            .FirstOrDefault(ua => ua.AssignmentId == id && ua.User.Role == "admin");
    }

    public void UpdateManager(int assignmentId, int userId, string username, string email, string gender, int? branchId, bool isActive)
    {
        var assignment = _context.UserAssignments.Find(assignmentId);
        if (assignment == null)
            throw new Exception("Manager assignment not found");

        var user = _context.Users.Find(userId);
        if (user == null)
            throw new Exception("User not found");

        user.Username = username;
        user.Email = email;
        user.Gender = gender;
        user.Isactive = isActive;

        assignment.BranchId = branchId;
        assignment.Isactive = isActive;

        _context.SaveChanges();
    }

    public void DeactivateManager(int assignmentId)
    {
        var assignment = _context.UserAssignments.Find(assignmentId);
        if (assignment == null)
            throw new Exception("Manager assignment not found");

        var user = _context.Users.Find(assignment.UserId);
        if (user == null)
            throw new Exception("User not found");

        user.Isactive = false;
        assignment.Isactive = false;

        _context.SaveChanges();
    }
    
    public UserAssignment GetUserBranchAssignment(int userId)
    {
        return _context.UserAssignments
            .Include(ua => ua.Branch)
            .Include(ua => ua.User)
            .FirstOrDefault(ua => ua.UserId == userId && ua.User.Role == "admin");
    }



}
