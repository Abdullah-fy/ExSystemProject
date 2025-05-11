using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminManagerController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminManagerController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminManager
        public IActionResult Index(string searchString = null, bool? activeOnly = null)
        {
            var userId = GetCurrentUserId();

            try
            {
                List<UserAssignment> managers = _unitOfWork.userAssignmentRepo.GetAllManagers(activeOnly);

                if (!string.IsNullOrEmpty(searchString))
                {
                    managers = managers.Where(m =>
                        (m.User != null && (
                            m.User.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                            m.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        )) ||
                        (m.Branch != null && m.Branch.BranchName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                ViewBag.SearchString = searchString;
                ViewBag.ActiveOnly = activeOnly;

                var managerDTOs = _mapper.Map<List<ManagerDTO>>(managers);
                return View(managerDTOs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AdminManager/Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading branch managers.";
                return View(new List<ManagerDTO>());
            }
        }


        // GET: AdminManager/Details/5
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();

            var manager = _unitOfWork.userAssignmentRepo.GetManagerById(id);
            if (manager == null)
                return NotFound();

            var managerDTO = _mapper.Map<ManagerDTO>(manager);
            return View(managerDTO);
        }

        // GET: AdminManager/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();

            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

            return View(new ManagerDTO { Isactive = true });
        }


        // POST: AdminManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ManagerDTO managerDTO, string Password)
        {
            var userId = GetCurrentUserId();

            try
            {
                if (string.IsNullOrEmpty(Password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                    var branchOptions = _unitOfWork.branchRepo.getAll();
                    ViewBag.Branches = new SelectList(branchOptions, "BranchId", "BranchName");
                    return View(managerDTO);
                }

                if (ModelState.IsValid)
                {
                    var user = new User
                    {
                        Username = managerDTO.Username,
                        Email = managerDTO.Email,
                        Gender = managerDTO.Gender,
                        Upassword = BCrypt.Net.BCrypt.HashPassword(Password),
                        Role = "admin",
                        Isactive = managerDTO.Isactive ?? true
                    };

                    _unitOfWork.userRepo.add(user);
                    _unitOfWork.save();

                    var userAssignment = new UserAssignment
                    {
                        UserId = user.UserId,
                        BranchId = managerDTO.BranchId,
                        Isactive = managerDTO.Isactive ?? true
                    };

                    _unitOfWork.userAssignmentRepo.add(userAssignment);
                    _unitOfWork.save();

                    TempData["SuccessMessage"] = "Branch Manager created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating branch manager: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error in Create Branch Manager: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            var allBranches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(allBranches, "BranchId", "BranchName");
            return View(managerDTO);
        }



        // GET: AdminManager/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();

            var manager = _unitOfWork.userAssignmentRepo.GetManagerById(id);
            if (manager == null)
                return NotFound();

            var managerDTO = _mapper.Map<ManagerDTO>(manager);

            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", managerDTO.BranchId);

            return View(managerDTO);
        }

        // POST: AdminManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ManagerDTO managerDTO)
        {
            var userId = GetCurrentUserId();

            if (id != managerDTO.AssignmentId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update both user info and assignment
                    _unitOfWork.userAssignmentRepo.UpdateManager(
                        managerDTO.AssignmentId,
                        managerDTO.UserId,
                        managerDTO.Username,
                        managerDTO.Email,
                        managerDTO.Gender,
                        managerDTO.BranchId,
                        managerDTO.Isactive ?? true
                    );

                    TempData["SuccessMessage"] = "Branch Manager updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = managerDTO.AssignmentId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating branch manager: {ex.Message}");
                }
            }

            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", managerDTO.BranchId);

            return View(managerDTO);
        }

        // GET: AdminManager/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

            var manager = _unitOfWork.userAssignmentRepo.GetManagerById(id);
            if (manager == null)
                return NotFound();

            var managerDTO = _mapper.Map<ManagerDTO>(manager);
            return View(managerDTO);
        }

        // POST: AdminManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                _unitOfWork.userAssignmentRepo.DeactivateManager(id);

                TempData["SuccessMessage"] = "Branch Manager deactivated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deactivating branch manager: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }
    }
}
