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
    [Authorize(Roles = "admin")]
    public class BranchManagerTrackController : BranchManagerBaseController
    {
        public BranchManagerTrackController(UnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        // GET: BranchManagerTrack
        public IActionResult Index(bool? active = true)
        {
            ViewData["Title"] = "Track Management";

            // Get tracks for this branch
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId);

            // Filter by active status if specified
            if (active.HasValue)
            {
                tracks = tracks.Where(t => t.IsActive == active.Value).ToList();
            }

            return View(tracks);
        }

        // GET: BranchManagerTrack/Details/5
        public IActionResult Details(int id)
        {
            var track = _unitOfWork.trackRepo.getById(id);

            // Verify track exists and belongs to this branch
            if (track == null || track.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get related data
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackWithBranch(id, null);
            var students = _unitOfWork.studentRepo.GetStudentsByDepartmentWithBranch(id, null);

            ViewBag.Instructors = instructors;
            ViewBag.Students = students;
            ViewBag.InstructorCount = instructors.Count();
            ViewBag.StudentCount = students.Count();

            ViewData["Title"] = $"Track: {track.TrackName}";
            return View(track);
        }

        // GET: BranchManagerTrack/Create
        public IActionResult Create()
        {
            // Set the branch ID to current branch
            var track = new Track { BranchId = CurrentBranchId };

            ViewData["Title"] = "Create New Track";
            ViewBag.BranchName = CurrentBranchName;

            return View(track);
        }

        // POST: BranchManagerTrack/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Track track)
        {
            // Always set the branch ID to current branch for security
            track.BranchId = CurrentBranchId;

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.trackRepo.add(track);
                    _unitOfWork.save();

                    TempData["Success"] = "Track created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating track: {ex.Message}");
                }
            }

            ViewData["Title"] = "Create New Track";
            ViewBag.BranchName = CurrentBranchName;
            return View(track);
        }

        // GET: BranchManagerTrack/Edit/5
        public IActionResult Edit(int id)
        {
            var track = _unitOfWork.trackRepo.getById(id);

            // Verify track exists and belongs to this branch
            if (track == null || track.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Track: {track.TrackName}";
            ViewBag.BranchName = CurrentBranchName;

            return View(track);
        }

        // POST: BranchManagerTrack/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Track track, string IsActive)
        {
            if (id != track.TrackId)
            {
                return NotFound();
            }

            // Always set the branch ID to current branch for security
            track.BranchId = CurrentBranchId;

            // Parse the IsActive value from form
            track.IsActive = IsActive?.ToLower() == "true";

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.trackRepo.update(track);
                    _unitOfWork.save();

                    TempData["Success"] = "Track updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating track: {ex.Message}");
                }
            }

            ViewData["Title"] = $"Edit Track: {track.TrackName}";
            ViewBag.BranchName = CurrentBranchName;
            return View(track);
        }


        // GET: BranchManagerTrack/Delete/5
        public IActionResult Delete(int id)
        {
            var track = _unitOfWork.trackRepo.getById(id);

            // Verify track exists and belongs to this branch
            if (track == null || track.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Track: {track.TrackName}";
            return View(track);
        }

        // POST: BranchManagerTrack/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var track = _unitOfWork.trackRepo.getById(id);

            // Verify track exists and belongs to this branch
            if (track == null || track.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            try
            {
                // Soft delete by setting IsActive to false
                track.IsActive = false;
                _unitOfWork.trackRepo.update(track);
                _unitOfWork.save();

                TempData["Success"] = "Track deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting track: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }



        // POST: BranchManagerTrack/ToggleStatus/5
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            try
            {
                var track = _unitOfWork.trackRepo.getById(id);

                // Verify track exists and belongs to this branch
                if (track == null || track.BranchId != CurrentBranchId)
                {
                    return Json(new { success = false, message = "Track not found or access denied" });
                }

                // Toggle the active status
                track.IsActive = !(track.IsActive ?? true);
                _unitOfWork.trackRepo.update(track);
                _unitOfWork.save();

                string statusText = track.IsActive == true ? "activated" : "deactivated";
                return Json(new
                {
                    success = true,
                    message = $"Track {statusText} successfully",
                    isActive = track.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: BranchManagerTrack/Students/5
        public IActionResult Students(int id)
        {
            var track = _unitOfWork.trackRepo.getById(id);

            // Verify track exists and belongs to this branch
            if (track == null || track.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            var students = _unitOfWork.studentRepo.GetStudentsByDepartmentWithBranch(id, null);

            ViewBag.Track = track;
            ViewData["Title"] = $"Students in {track.TrackName}";

            return View(students);
        }

        // GET: BranchManagerTrack/Instructors/5
        public IActionResult Instructors(int id)
        {
            var track = _unitOfWork.trackRepo.getById(id);

            // Verify track exists and belongs to this branch
            if (track == null || track.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            var instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackWithBranch(id, null);

            ViewBag.Track = track;
            ViewData["Title"] = $"Instructors in {track.TrackName}";

            return View(instructors);
        }
    }
}
