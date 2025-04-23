using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.Controllers
{
    public class AdminBranchController : Controller
    {
        public UnitOfWork unit { get; set; }
        public AdminBranchController(UnitOfWork _unit)
        {
            unit = _unit;
        }
        [HttpGet]
        public IActionResult Index(int page = 1)
        {
            var branches = unit.adminBranchRepo.GetAll();
            int pageSize = 6;

            var Branches = branches.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(branches.Count() / (double)pageSize);

            return View(Branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var instrutors = unit.instructorRepo.getAllWithUserData();
            ViewBag.instructors = instrutors;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Branch branch, int managerId)
        {
            if (ModelState.IsValid)
            {
                unit.adminBranchRepo.CreateBranch(branch);
                unit.save();

                ViewBag.SuccessMessage = "Branch Added Succefully";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            List<Track> tracks = unit.adminBranchRepo.GetTracksByBranchId(id);
            Branch branch = unit.adminBranchRepo.GetBranchById(id);
            if (branch == null) return NotFound();
            ViewBag.tracks = tracks;
            return View(branch);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (id == null) return BadRequest();
            ViewBag.id = id;
            return View();
        }

        public IActionResult ConfirmDelete(int id)
        {
            if (id == null) return BadRequest();
            unit.adminBranchRepo.DeleteBranch(id);
            unit.save();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id == null) return BadRequest();
            Branch branch = unit.adminBranchRepo.GetBranchById(id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost]
        public IActionResult Edit(Branch branch)
        {
            if (ModelState.IsValid)
            {
                unit.adminBranchRepo.UpdateBranch(branch);
                unit.save();
                return RedirectToAction("Index");
            }

            return View(branch);
        }
    }
}
