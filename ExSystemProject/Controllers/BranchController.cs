using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class BranchController : SuperAdminBaseController
    {
        public BranchController(UnitOfWork unitOfWork) : base(unitOfWork) { }

        [HttpGet]
        public IActionResult Index(string searchString, bool activeOnly = false, int page = 1)
        {
            var userId = GetCurrentUserId();

            var query = _unitOfWork.branchRepo.getAll().AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b =>
                    b.BranchName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.Location.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (activeOnly)
            {
                query = query.Where(b => b.Isactive == true);
            }

            int pageSize = 6;
            var totalItems = query.Count();
            var branches = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentFilter = searchString;
            ViewBag.ActiveOnly = activeOnly;

            return View(branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();

            var instrutors = _unitOfWork.instructorRepo.getAllWithUserData();
            ViewBag.instructors = instrutors;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Branch branch, int managerId)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                _unitOfWork.branchRepo.add(branch);
                _unitOfWork.save();

                ViewBag.SuccessMessage = "Branch Added Succefully";
                return RedirectToAction("Index");
            }

            return View();
            
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();

            List<Track> tracks = _unitOfWork.branchRepo.GetTracksByBranchId(id);
            Branch branch = _unitOfWork.branchRepo.getById(id);
            if (branch == null) return NotFound();
            ViewBag.tracks = tracks;
            return View(branch);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            ViewBag.id = id;

            var branch = _unitOfWork.branchRepo.GetBranchById(id);
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(id);
            ViewBag.tracks = tracks;

            return View(branch);
        }
        
        public IActionResult ConfirmDelete(int id)
        {
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            _unitOfWork.branchRepo.delete(id);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            Branch branch = _unitOfWork.branchRepo.getById(id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost]
        public IActionResult Edit(Branch branch)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                _unitOfWork.branchRepo.update(branch);
                _unitOfWork.save();
                return RedirectToAction("Index");
            }

            return View(branch);
        }


    }
}
