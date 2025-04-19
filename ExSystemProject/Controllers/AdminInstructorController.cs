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
        public IActionResult Index(int? branchId = null, int? trackId = null, bool? activeOnly = null)
        {
            List<Instructor> instructors;

            if (branchId.HasValue)
            {
                // Get instructors for a specific branch
                instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchId(branchId.Value, activeOnly);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = _unitOfWork.branchRepo.getById(branchId.Value)?.BranchName;
            }
            else if (trackId.HasValue)
            {
                // Get instructors for a specific track
                instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackId(trackId.Value, activeOnly);
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = _unitOfWork.trackRepo.getById(trackId.Value)?.TrackName;
            }
            else
            {
                // Get all instructors
                instructors = _unitOfWork.instructorRepo.GetAllInstructors(activeOnly);
            }

            var branches = _unitOfWork.branchRepo.getAll();
            var tracks = _unitOfWork.trackRepo.getAll();

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
            var coursesWithStudentCount = _unitOfWork.instructorRepo.GetInstructorCoursesWithStudentCount(id);
            ViewBag.CoursesReport = coursesWithStudentCount;

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Create
        public IActionResult Create()
        {
            var tracks = _unitOfWork.trackRepo.getAll();
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName");

            return View();
        }

        // POST: AdminInstructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InstructorDTO instructorDTO)
        {
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
            var tracks = _unitOfWork.trackRepo.getAll();
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Edit/5
        public IActionResult Edit(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorById(id);
            if (instructor == null)
                return NotFound();

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            var tracks = _unitOfWork.trackRepo.getAll();
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
            var tracks = _unitOfWork.trackRepo.getAll();
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
    }
}