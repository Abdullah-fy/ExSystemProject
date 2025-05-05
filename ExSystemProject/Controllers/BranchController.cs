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
        public IActionResult Index(int page = 1)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var branches = _unitOfWork.branchRepo.getAll();
            int pageSize = 6;

            var Branches = branches.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(branches.Count() / (double)pageSize);



            return View(Branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var instrutors = _unitOfWork.instructorRepo.getAllWithUserData();
            ViewBag.instructors = instrutors;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Branch branch, int managerId)
        {
            // Get current user ID using the base controller method
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
            // Get current user ID using the base controller method
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
            _unitOfWork.branchRepo.delete(id);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            if (id == null) return BadRequest();
            Branch branch = _unitOfWork.branchRepo.getById(id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost]
        public IActionResult Edit(Branch branch)
        {
            // Get current user ID using the base controller method
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
