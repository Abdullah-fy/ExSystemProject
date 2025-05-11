using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class TrackController : SuperAdminBaseController
    {
        public TrackController(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public IActionResult Index(string searchString, bool activeOnly = false, int page = 1)
        {
            var userId = GetCurrentUserId();
            var query = _unitOfWork.trackRepo.GetAllWithBranch().AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t =>
                    t.track_name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    t.branch_name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (activeOnly)
            {
                query = query.Where(t => t.is_active == true);
            }

            int pageSize = 6;
            var totalItems = query.Count();
            var tracks = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentFilter = searchString;
            ViewBag.ActiveOnly = activeOnly;

            return View(tracks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();

            List<Branch> branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = branches;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Track track)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                if (track.BranchId == 0)
                {
                    ViewBag.Branches = _unitOfWork.branchRepo.getAll();
                    return View();
                }
                _unitOfWork.trackRepo.add(track);
                _unitOfWork.save();
                return RedirectToAction("Index", "Branch");
            }
            ViewBag.Branches = _unitOfWork.branchRepo.getAll();
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            ViewBag.id = id;
            return View();
        }

        public IActionResult ConfirmDelete(int id)
        {
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            _unitOfWork.trackRepo.delete(id);
            _unitOfWork.save();
            return RedirectToAction("Details", "Branch", new { id = ViewBag.BranchId });
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            Track track = _unitOfWork.trackRepo.getById(id);
            if (track == null) return NotFound();

            ViewBag.Branches = _unitOfWork.branchRepo.getAll();

            return View(track);
        }

        [HttpPost]
        public IActionResult Edit(Track track)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                _unitOfWork.trackRepo.update(track);
                _unitOfWork.save();
                return RedirectToAction("Index", "Branch");
            }

            return View(track);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ViewBag.StudentCount = _unitOfWork.trackRepo.GetStudentsByTrackId(id).Count();
            var track = _unitOfWork.trackRepo.GetTrackById(id); 
            return View(track); 
        }

    }
}
