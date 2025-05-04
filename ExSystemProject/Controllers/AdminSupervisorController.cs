// ExSystemProject/Controllers/AdminSupervisorController.cs
using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using ExSystemProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminSupervisorController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminSupervisorController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminSupervisor
        public IActionResult Index(bool? active = null)
        {
            ViewData["Title"] = "Supervisors Management";

            var supervisors = _unitOfWork.supervisorRepo.GetAllSupervisors(active);
            var supervisorDTOs = _mapper.Map<List<SupervisorDTO>>(supervisors);

            return View(supervisorDTOs);
        }

        // GET: AdminSupervisor/Details/5
        public IActionResult Details(int id)
        {
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            if (supervisor == null)
            {
                return NotFound();
            }

            var supervisorDTO = _mapper.Map<SupervisorDTO>(supervisor);

            // Get additional data
            var students = _unitOfWork.supervisorRepo.GetStudentsUnderSupervisor(id);
            var instructors = _unitOfWork.supervisorRepo.GetInstructorsUnderSupervisor(id);
            var courses = _unitOfWork.supervisorRepo.GetCoursesUnderSupervisor(id);
            var exams = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(id);

            ViewBag.Students = students;
            ViewBag.Instructors = instructors;
            ViewBag.Courses = courses;
            ViewBag.Exams = exams;

            ViewData["Title"] = $"Supervisor: {supervisor.User?.Username}";

            return View(supervisorDTO);
        }

        // GET: AdminSupervisor/Create
        public IActionResult Create()
        {
            // Get users who can be supervisors (users with role "supervisor")
            var users = _unitOfWork.userRepo.getAll()
                .Where(u => u.Role == "supervisor" && u.Isactive == true)
                .ToList();

            // Get all branches
            var branches = _unitOfWork.branchRepo.getAll()
                .Where(b => b.Isactive == true)
                .ToList();

            ViewBag.Users = new SelectList(users, "UserId", "Username");
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

            // Tracks will be populated via AJAX based on selected branch

            ViewData["Title"] = "Create New Supervisor Assignment";

            return View(new SupervisorDTO { IsActive = true });
        }

        // POST: AdminSupervisor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SupervisorDTO supervisorDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // First check if a user with this email already exists
                    if (_unitOfWork.userRepo.getAll().Any(u => u.Email == supervisorDTO.Email))
                    {
                        ModelState.AddModelError("Email", "This email address is already in use");

                        // Repopulate dropdowns
                        var branches = _unitOfWork.branchRepo.getAll()
                            .Where(b => b.Isactive == true)
                            .ToList();

                        ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

                        return View(supervisorDTO);
                    }

                    // Create a new user with supervisor role
                    var user = new User
                    {
                        Username = supervisorDTO.Username,
                        Email = supervisorDTO.Email,
                        Gender = supervisorDTO.Gender,
                        Upassword = BCrypt.Net.BCrypt.HashPassword(supervisorDTO.Password),
                        Role = "supervisor",
                        Isactive = supervisorDTO.IsActive
                    };

                    _unitOfWork.userRepo.add(user);
                    _unitOfWork.save();

                    // Create supervisor assignment
                    var assignment = _unitOfWork.supervisorRepo.CreateSupervisor(
                        user.UserId,
                        supervisorDTO.BranchId ?? 0,
                        supervisorDTO.TrackId);

                    TempData["Success"] = "Supervisor created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating supervisor: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            var branchesForSelect = _unitOfWork.branchRepo.getAll()
                .Where(b => b.Isactive == true)
                .ToList();

            ViewBag.Branches = new SelectList(branchesForSelect, "BranchId", "BranchName", supervisorDTO.BranchId);

            return View(supervisorDTO);
        }


        // GET: AdminSupervisor/Edit/5
        public IActionResult Edit(int id)
        {
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            if (supervisor == null)
            {
                return NotFound();
            }

            var supervisorDTO = _mapper.Map<SupervisorDTO>(supervisor);

            var branches = _unitOfWork.branchRepo.getAll()
                .Where(b => b.Isactive == true)
                .ToList();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", supervisorDTO.BranchId);

            if (supervisorDTO.BranchId.HasValue)
            {
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(supervisorDTO.BranchId.Value)
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", supervisorDTO.TrackId);
            }

            ViewData["Title"] = $"Edit Supervisor: {supervisor.User?.Username}";

            return View(supervisorDTO);
        }

        // POST: AdminSupervisor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, SupervisorDTO supervisorDTO)
        {
            if (id != supervisorDTO.AssignmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.supervisorRepo.UpdateSupervisor(
                        supervisorDTO.AssignmentId,
                        supervisorDTO.BranchId,
                        supervisorDTO.TrackId,
                        supervisorDTO.IsActive );

                    TempData["Success"] = "Supervisor assignment updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating supervisor assignment: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay form
            var branches = _unitOfWork.branchRepo.getAll()
                .Where(b => b.Isactive == true)
                .ToList();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", supervisorDTO.BranchId);

            if (supervisorDTO.BranchId.HasValue)
            {
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(supervisorDTO.BranchId.Value)
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", supervisorDTO.TrackId);
            }

            ViewData["Title"] = "Edit Supervisor Assignment";

            return View(supervisorDTO);
        }

        // GET: AdminSupervisor/Delete/5
        public IActionResult Delete(int id)
        {
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            if (supervisor == null)
            {
                return NotFound();
            }

            var supervisorDTO = _mapper.Map<SupervisorDTO>(supervisor);

            ViewData["Title"] = $"Delete Supervisor: {supervisor.User?.Username}";

            return View(supervisorDTO);
        }

        // POST: AdminSupervisor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _unitOfWork.supervisorRepo.DeactivateSupervisor(id);

                TempData["Success"] = "Supervisor deactivated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating supervisor: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: AdminSupervisor/GetTracksByBranch/5
        [HttpGet]
        public JsonResult GetTracksByBranch(int branchId)
        {
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(branchId)
                .Where(t => t.IsActive == true)
                .Select(t => new { id = t.TrackId, name = t.TrackName })
                .ToList();

            return Json(tracks);
        }



    }
}
