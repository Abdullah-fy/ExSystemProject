using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.Controllers
{
    public class TrackController : Controller
    {
        UnitOfWork unit;
        public TrackController(UnitOfWork _unit)
        {
            this.unit = _unit;
        }
        public IActionResult Index(int page = 1)
        {
            var tracks = unit.trackRepo.GetAllWithBranch();
            int pageSize = 6;

            var Branches = tracks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(tracks.Count() / (double)pageSize);

            return View(Branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            List<Branch> branches = unit.branchRepo.getAll();
            ViewBag.Branches = branches;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Track track)
        {
            if (ModelState.IsValid)
            {
                if(track.BranchId == 0)
                {
                    ViewBag.Branches = unit.branchRepo.getAll();
                    return View();
                }
                unit.trackRepo.add(track);
                unit.save();
                return RedirectToAction("Index", "Branch");
            }
            ViewBag.Branches = unit.branchRepo.getAll();
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
            unit.trackRepo.delete(id);
            unit.save();
            return RedirectToAction("Details", "Branch", new {id = ViewBag.BranchId});
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id == null) return BadRequest();
            Track track = unit.trackRepo.getById(id);
            if (track == null) return NotFound();

            ViewBag.Branches = unit.branchRepo.getAll();

            return View(track);
        }

        [HttpPost]
        public IActionResult Edit(Track track)
        {

            if (ModelState.IsValid)
            {
                unit.trackRepo.update(track);
                unit.save();
                return RedirectToAction("Index", "Branch");
            }

            return View(track);
        }


    }
}
