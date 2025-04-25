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

        public IActionResult Index(int page = 1)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var tracks = _unitOfWork.trackRepo.GetAllWithBranch();
            int pageSize = 6;

            var Branches = tracks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(tracks.Count() / (double)pageSize);

            return View(Branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            List<Branch> branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = branches;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Track track)
        {
            // Get current user ID using the base controller method
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
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            ViewBag.id = id;
            return View();
        }

        public IActionResult ConfirmDelete(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            _unitOfWork.trackRepo.delete(id);
            _unitOfWork.save();
            return RedirectToAction("Details", "Branch", new { id = ViewBag.BranchId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Get current user ID using the base controller method
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
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                _unitOfWork.trackRepo.update(track);
                _unitOfWork.save();
                return RedirectToAction("Index", "Branch");
            }

            return View(track);
        }
    }
}
