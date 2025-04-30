using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using ExSystemProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class BranchManagerSupervisorController : BranchManagerBaseController
    {
        private readonly IMapper _mapper;

        public BranchManagerSupervisorController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: BranchManagerSupervisor
        public IActionResult Index(bool? active = true)
        {
            ViewData["Title"] = "Supervisors Management";

            // Get supervisors for this branch
            var supervisors = _unitOfWork.supervisorRepo.GetSupervisorsByBranchId(CurrentBranchId, active);

            return View(supervisors);
        }

        // GET: BranchManagerSupervisor/Details/5
        public IActionResult Details(int id)
        {
            // Get supervisor by id
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            // Verify supervisor exists and belongs to this branch
            if (supervisor == null || supervisor.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = "Supervisor Details";
            return View(supervisor);
        }

        // GET: BranchManagerSupervisor/Create
        public IActionResult Create()
        {
            try
            {
                // Get tracks from the current branch for optional assignment
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TrackId.ToString(),
                        Text = t.TrackName
                    })
                    .ToList();

                // Get branch details directly from the database
                var branch = _unitOfWork.branchRepo.getById(CurrentBranchId);
                string branchName = branch?.BranchName;

                if (string.IsNullOrEmpty(branchName))
                {
                    // Fallback to the current branch name from the base controller
                    branchName = CurrentBranchName ?? "Default Branch";
                }

                var model = new SupervisorViewModel
                {
                    BranchId = CurrentBranchId,
                    BranchName = branchName,
                    IsActive = true,
                    Tracks = tracks
                };

                ViewData["Title"] = "Create New Supervisor";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading form: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BranchManagerSupervisor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SupervisorViewModel model)
        {
            try
            {
                // For debugging
                System.Diagnostics.Debug.WriteLine($"Model: Username={model.Username}, Email={model.Email}, BranchId={model.BranchId}, BranchName={model.BranchName}");

                // Force set the BranchId to the current branch ID for security
                model.BranchId = CurrentBranchId;

                // Get branch details directly from the database
                var branch = _unitOfWork.branchRepo.getById(CurrentBranchId);
                if (branch != null)
                {
                    model.BranchName = branch.BranchName;
                }

                // Check for email uniqueness
                if (_unitOfWork.userRepo.getAll().Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email address is already in use");

                    // Repopulate tracks dropdown
                    var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                        .Where(t => t.IsActive == true)
                        .Select(t => new SelectListItem
                        {
                            Value = t.TrackId.ToString(),
                            Text = t.TrackName,
                            Selected = t.TrackId == model.TrackId
                        })
                        .ToList();

                    model.Tracks = tracks;
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    // Create user with supervisor role
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        Gender = model.Gender,
                        Upassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                        Role = "supervisor",
                        Isactive = model.IsActive
                    };

                    _unitOfWork.userRepo.add(user);
                    _unitOfWork.save();

                    System.Diagnostics.Debug.WriteLine($"User created with ID: {user.UserId}");

                    // Create supervisor assignment
                    var assignment = new UserAssignment
                    {
                        UserId = user.UserId,
                        BranchId = CurrentBranchId,
                        TrackId = model.TrackId,
                        Isactive = model.IsActive
                    };

                    _unitOfWork.userAssignmentRepo.add(assignment);
                    _unitOfWork.save();

                    System.Diagnostics.Debug.WriteLine($"Assignment created with ID: {assignment.AssignmentId}");

                    TempData["Success"] = "Supervisor created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // If we get here, something went wrong with validation
                System.Diagnostics.Debug.WriteLine("Model validation failed:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"- {error.ErrorMessage}");
                }

                // Repopulate tracks dropdown
                var tracksList = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TrackId.ToString(),
                        Text = t.TrackName,
                        Selected = t.TrackId == model.TrackId
                    })
                    .ToList();

                model.Tracks = tracksList;
                return View(model);
            }
            catch (Exception ex)
            {
                // Enhanced error logging
                System.Diagnostics.Debug.WriteLine($"Error creating supervisor: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"Error creating supervisor: {ex.Message}");

                // Repopulate tracks dropdown
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TrackId.ToString(),
                        Text = t.TrackName,
                        Selected = t.TrackId == model.TrackId
                    })
                    .ToList();

                model.Tracks = tracks;
                return View(model);
            }
        }




        // GET: BranchManagerSupervisor/Edit/5
        public IActionResult Edit(int id)
        {
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            // Verify supervisor exists and belongs to this branch
            if (supervisor == null || supervisor.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get tracks from the current branch
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName,
                    Selected = t.TrackId == supervisor.TrackId
                })
                .ToList();

            var model = new SupervisorEditViewModel
            {
                AssignmentId = supervisor.AssignmentId,
                UserId = supervisor.UserId,
                Username = supervisor.User?.Username,
                Email = supervisor.User?.Email,
                Gender = supervisor.User?.Gender,
                BranchId = CurrentBranchId,
                BranchName = CurrentBranchName,
                TrackId = supervisor.TrackId,
                IsActive = supervisor.Isactive ?? true,
                Tracks = tracks
            };

            ViewData["Title"] = "Edit Supervisor";
            return View(model);
        }

        // POST: BranchManagerSupervisor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, SupervisorEditViewModel model)
        {
            if (id != model.AssignmentId)
            {
                return NotFound();
            }

            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            // Verify supervisor exists and belongs to this branch
            if (supervisor == null || supervisor.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update supervisor
                    _unitOfWork.supervisorRepo.UpdateSupervisor(
                        model.AssignmentId,
                        model.UserId,
                        model.Username,
                        model.Email,
                        model.BranchId,
                        model.IsActive
                    );

                    // If track changed, update it separately
                    if (supervisor.TrackId != model.TrackId)
                    {
                        supervisor.TrackId = model.TrackId;
                        _unitOfWork.save();
                    }

                    TempData["Success"] = "Supervisor updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.AssignmentId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating supervisor: {ex.Message}");
                }
            }

            // If we get here, repopulate tracks and return view
            model.BranchName = CurrentBranchName;
            model.Tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName,
                    Selected = t.TrackId == model.TrackId
                })
                .ToList();

            return View(model);
        }

        // GET: BranchManagerSupervisor/Delete/5
        public IActionResult Delete(int id)
        {
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            // Verify supervisor exists and belongs to this branch
            if (supervisor == null || supervisor.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = "Delete Supervisor";
            return View(supervisor);
        }

        // POST: BranchManagerSupervisor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

            // Verify supervisor exists and belongs to this branch
            if (supervisor == null || supervisor.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.supervisorRepo.DeactivateSupervisor(id);
                TempData["Success"] = "Supervisor deactivated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating supervisor: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // POST: BranchManagerSupervisor/ToggleStatus/5
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            try
            {
                var supervisor = _unitOfWork.supervisorRepo.GetSupervisorById(id);

                // Verify supervisor exists and belongs to this branch
                if (supervisor == null || supervisor.BranchId != CurrentBranchId)
                {
                    return Json(new { success = false, message = "Supervisor not found or access denied" });
                }

                // Toggle active status
                supervisor.Isactive = !(supervisor.Isactive ?? true);
                if (supervisor.User != null)
                {
                    supervisor.User.Isactive = supervisor.Isactive;
                }
                _unitOfWork.save();

                string statusText = supervisor.Isactive == true ? "activated" : "deactivated";
                return Json(new
                {
                    success = true,
                    message = $"Supervisor {statusText} successfully",
                    isActive = supervisor.Isactive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
