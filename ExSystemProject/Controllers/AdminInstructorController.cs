using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    public class AdminInstructorController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminInstructorController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        // GET: AdminInstructor
        public IActionResult Index(int? branchId = null, int? trackId = null, bool? activeOnly = true)
        {
            List<Instructor> instructors;
            var branches = _unitOfWork.branchRepo.GetAllActive();
            var tracks = new List<Track>();

            if (branchId.HasValue)
            {
                // Get tracks for selected branch
                tracks = _unitOfWork.branchRepo.GetTracksByBranchId(branchId.Value);

                // Get instructors for a specific branch
                instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchId(branchId.Value, activeOnly);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = _unitOfWork.branchRepo.getById(branchId.Value)?.BranchName;
            }
            else if (trackId.HasValue)
            {
                // Get the track to determine its branch
                var track = _unitOfWork.trackRepo.getById(trackId.Value);
                if (track?.BranchId != null)
                {
                    // Load tracks for this branch
                    tracks = _unitOfWork.branchRepo.GetTracksByBranchId(track.BranchId.Value);
                    ViewBag.BranchId = track.BranchId;
                }
                else
                {
                    // Fallback to all tracks
                    tracks = _unitOfWork.trackRepo.getAll();
                }

                // Get instructors for a specific track
                instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackId(trackId.Value, activeOnly);
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = track?.TrackName;
            }
            else
            {
                // No filters applied, load all tracks
                tracks = _unitOfWork.trackRepo.getAll();

                // Get all instructors
                instructors = _unitOfWork.instructorRepo.GetAllInstructors(activeOnly);
            }

            // Create Select lists for dropdowns
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", branchId);
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", trackId);
            ViewBag.ActiveOnly = activeOnly;

            var instructorDTOs = _mapper.Map<List<InstructorDTO>>(instructors);
            return View(instructorDTOs);
        }


        // GET: AdminInstructor/Details/5
        public IActionResult Details(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorById(id);
            if (instructor == null)
                return NotFound();

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            // Get instructor courses with student count
            var coursesData = _unitOfWork.instructorRepo.GetInstructorCoursesWithStudentCount(id);
            ViewBag.CoursesReport = coursesData["Courses"];
            ViewBag.CoursesSummary = coursesData["Summary"];

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Create
        public IActionResult Create()
        {
            // Get branches for dropdown
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

            // Get tracks for dropdown - initially empty, will be populated via JS when branch is selected
            ViewBag.Tracks = new SelectList(new List<Track>(), "TrackId", "TrackName");

            return View();
        }

        // POST: AdminInstructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InstructorDTO instructorDTO)
        {
            // Remove validation for fields that don't need to be required
            if (ModelState.ContainsKey("TrackName")) ModelState.Remove("TrackName");
            if (ModelState.ContainsKey("BranchName")) ModelState.Remove("BranchName");
            if (ModelState.ContainsKey("ImageUrl")) ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // Create instructor using stored procedure
                    _unitOfWork.instructorRepo.CreateInstructor(
                        instructorDTO.Username,
                        instructorDTO.Email,
                        instructorDTO.Gender,
                        "defaultPassword123", // Consider generating a random password or requiring it in the form
                        instructorDTO.Salary ?? 0,
                        instructorDTO.TrackId ?? 0
                    );

                    TempData["SuccessMessage"] = "Instructor created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating instructor: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", instructorDTO.BranchId);

            if (instructorDTO.BranchId.HasValue)
            {
                var tracks = _unitOfWork.trackRepo.getAll().Where(t => t.BranchId == instructorDTO.BranchId.Value);
                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);
            }
            else
            {
                ViewBag.Tracks = new SelectList(new List<Track>(), "TrackId", "TrackName");
            }

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Edit/5
        public IActionResult Edit(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorById(id);
            if (instructor == null)
                return NotFound();

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            // Load branch information
            int? branchId = null;
            if (instructor.Track != null && instructor.Track.BranchId.HasValue)
            {
                branchId = instructor.Track.BranchId.Value;
            }

            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", branchId);

            // Get tracks for selected branch
            var tracks = branchId.HasValue
                ? _unitOfWork.trackRepo.getAll().Where(t => t.BranchId == branchId)
                : _unitOfWork.trackRepo.getAll();

            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructor.TrackId);

            return View(instructorDTO);
        }

        // POST: AdminInstructor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InstructorDTO instructorDTO, string IsactiveHidden)
        {
            if (id != instructorDTO.InsId)
                return NotFound();

            // Remove validation for fields that don't need to be required
            if (ModelState.ContainsKey("TrackName")) ModelState.Remove("TrackName");
            if (ModelState.ContainsKey("BranchName")) ModelState.Remove("BranchName");
            if (ModelState.ContainsKey("ImageUrl")) ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // Explicitly set the active status based on the hidden field value
                    instructorDTO.Isactive = IsactiveHidden?.ToLower() == "true";

                    // Update instructor using stored procedure
                    _unitOfWork.instructorRepo.UpdateInstructor(
                        instructorDTO.InsId,
                        instructorDTO.Username,
                        instructorDTO.Email,
                        instructorDTO.Gender,
                        instructorDTO.Salary ?? 0,
                        instructorDTO.TrackId ?? 0,
                        instructorDTO.Isactive ?? true
                    );

                    TempData["SuccessMessage"] = "Instructor updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = instructorDTO.InsId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating instructor: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", instructorDTO.BranchId);

            var tracks = instructorDTO.BranchId.HasValue
                ? _unitOfWork.trackRepo.getAll().Where(t => t.BranchId == instructorDTO.BranchId)
                : _unitOfWork.trackRepo.getAll();

            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Delete/5
        public IActionResult Delete(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorById(id);
            if (instructor == null)
                return NotFound();

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);
            return View(instructorDTO);
        }

        // POST: AdminInstructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                // Delete instructor using stored procedure (logical delete)
                _unitOfWork.instructorRepo.DeleteInstructor(id);

                TempData["SuccessMessage"] = "Instructor deactivated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deactivating instructor: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        // GET: AdminInstructor/Courses/5
        public IActionResult Courses(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorById(id);
            if (instructor == null)
                return NotFound();

            var courses = _unitOfWork.instructorRepo.GetInstructorCourses(id);

            ViewBag.InstructorId = id;
            ViewBag.InstructorName = instructor.User?.Username;

            return View(courses);
        }

        // GET: AdminInstructor/GetTracksByBranch/5
        [HttpGet]
        public JsonResult GetTracksByBranch(int branchId)
        {
            // Get tracks for the specified branch
            var tracks = _unitOfWork.trackRepo.getAll()
                .Where(t => t.BranchId == branchId)
                .Select(t => new { t.TrackId, t.TrackName })
                .ToList();

            return Json(tracks);
        }
    }
}