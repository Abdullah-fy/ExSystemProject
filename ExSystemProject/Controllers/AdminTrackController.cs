using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.Controllers
{
    public class AdminTrackController : Controller
    {
        UnitOfWork unit;
        public AdminTrackController(UnitOfWork _unit)
        {
            this.unit = _unit;
        }
        public IActionResult Index(int page = 1)
        {
            var tracks = unit.adminTrackRepo.GetAllWithBranch();
            int pageSize = 6;

            var Branches = tracks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(tracks.Count() / (double)pageSize);

            return View(Branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            List<Branch> branches = unit.adminBranchRepo.GetAll();
            ViewBag.Branches = branches;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Track track)
        {
            if (ModelState.IsValid)
            {
                if (track.BranchId == 0)
                {
                    ViewBag.Branches = unit.adminBranchRepo.GetAll();
                    return View();
                }
                unit.adminTrackRepo.CreateTrack(track);
                unit.save();
                return RedirectToAction("Index", "Branch");
            }
            ViewBag.Branches = unit.adminBranchRepo.GetAll();
            return View();
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
            unit.adminTrackRepo.DeleteTrack(id);
            unit.save();
            return RedirectToAction("Details", "Branch", new { id = ViewBag.BranchId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id == null) return BadRequest();
            Track track = unit.adminTrackRepo.GetTrackById(id);
            if (track == null) return NotFound();

            ViewBag.Branches = unit.adminBranchRepo.GetAll();

            return View(track);
        }

        [HttpPost]
        public IActionResult Edit(Track track)
        {
            if (ModelState.IsValid)
            {
                unit.adminTrackRepo.UpdateTrack(track);
                unit.save();
                return RedirectToAction("Index", "Branch");
            }

            return View(track);
        }
    }
}
