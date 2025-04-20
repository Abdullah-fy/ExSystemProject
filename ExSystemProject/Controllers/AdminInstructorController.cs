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
using System.Diagnostics;

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
        public IActionResult Index(int? branchId = null, int? trackId = null, string searchString = null, bool? activeOnly = true)
        {
            List<Instructor> instructors;

            // Apply filters
            if (branchId.HasValue && trackId.HasValue)
            {
                instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackWithBranch(trackId.Value, activeOnly);
            }
            else if (branchId.HasValue)
            {
                instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(branchId.Value, activeOnly);
            }
            else
            {
                instructors = _unitOfWork.instructorRepo.GetAllInstructorsWithBranch(activeOnly);
            }

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                instructors = instructors.Where(i =>
                    i.User.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    i.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (i.Track != null && i.Track.TrackName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Map to DTOs
            var instructorDTOs = _mapper.Map<List<InstructorDTO>>(instructors);

            // Get branches for filtering
            ViewBag.Branches = _unitOfWork.branchRepo.getAll().Select(b => new SelectListItem
            {
                Text = b.BranchName,
                Value = b.BranchId.ToString()
            }).ToList();

            // Get tracks for the selected branch
            if (branchId.HasValue)
            {
                ViewBag.Tracks = _unitOfWork.trackRepo.GetTracksByBranchId(branchId.Value).Select(t => new SelectListItem
                {
                    Text = t.TrackName,
                    Value = t.TrackId.ToString()
                }).ToList();
            }

            // Set filter values for the view
            ViewBag.SelectedBranch = branchId;
            ViewBag.SelectedTrack = trackId;
            ViewBag.SearchString = searchString;
            ViewBag.ActiveOnly = activeOnly;

            return View(instructorDTOs);
        }

        // GET: AdminInstructor/Details/5
        public IActionResult Details(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            if (instructor == null)
            {
                return NotFound();
            }

            // Get instructor courses
            var courses = _unitOfWork.instructorRepo.GetInstructorCourses(id);

            // Map instructor to DTO
            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            // Map courses to DTOs
            instructorDTO.AssignedCourses = _mapper.Map<List<CourseDTO>>(courses);

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Create
        public IActionResult Create()
        {
            PopulateDropDowns();
            return View();
        }

        // POST: AdminInstructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InstructorDTO instructorDTO)
        {
            try
            {
                // Simple validation
                if (string.IsNullOrEmpty(instructorDTO.Username) ||
                    string.IsNullOrEmpty(instructorDTO.Email) ||
                    string.IsNullOrEmpty(instructorDTO.Gender) ||
                    !instructorDTO.TrackId.HasValue ||
                    !instructorDTO.Salary.HasValue)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields";
                    PopulateDropDowns();
                    return View(instructorDTO);
                }

                // Create instructor
                _unitOfWork.instructorRepo.CreateInstructor(
                    instructorDTO.Username,
                    instructorDTO.Email,
                    instructorDTO.Gender,
                    "DefaultPassword123!", // Default password
                    instructorDTO.Salary ?? 0,
                    instructorDTO.TrackId ?? 0
                );

                TempData["SuccessMessage"] = "Instructor created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Simple error handling
                TempData["ErrorMessage"] = "Error creating instructor: " + ex.Message;
                PopulateDropDowns();
                return View(instructorDTO);
            }
        }


        // GET: AdminInstructor/Edit/5
        public IActionResult Edit(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            if (instructor == null)
            {
                return NotFound();
            }

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            PopulateDropDowns(instructorDTO.BranchId);
            return View(instructorDTO);
        }

        // POST: AdminInstructor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InstructorDTO instructorDTO)
        {
            if (id != instructorDTO.InsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Use stored procedure to update instructor
                    _unitOfWork.instructorRepo.UpdateInstructor(
                        instructorDTO.InsId,
                        instructorDTO.Username,
                        instructorDTO.Email,
                        instructorDTO.Gender,
                        instructorDTO.Salary ?? 0,
                        instructorDTO.TrackId ?? 0,
                        instructorDTO.Isactive ?? true
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating instructor: {ex.Message}");
                }
            }

            PopulateDropDowns(instructorDTO.BranchId);
            return View(instructorDTO);
        }

        // GET: AdminInstructor/Delete/5
        public IActionResult Delete(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            if (instructor == null)
            {
                return NotFound();
            }

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
                _unitOfWork.instructorRepo.DeleteInstructor(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting instructor: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: AdminInstructor/Courses/5
        public IActionResult Courses(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            if (instructor == null)
            {
                return NotFound();
            }

            // Get instructor courses with student details
            var coursesData = _unitOfWork.instructorRepo.GetInstructorCoursesWithStudentCount(id);

            // Pass instructor data to view
            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            ViewBag.InstructorData = instructorDTO;
            ViewBag.CoursesData = coursesData;

            return View();
        }

        // Helper method to populate dropdowns for branches and tracks
        private void PopulateDropDowns(int? branchId = null)
        {
            // Get all branches
            ViewBag.Branches = _unitOfWork.branchRepo.getAll().Select(b => new SelectListItem
            {
                Text = b.BranchName,
                Value = b.BranchId.ToString()
            }).ToList();

            // Get tracks for selected branch
            if (branchId.HasValue)
            {
                ViewBag.Tracks = _unitOfWork.trackRepo.GetTracksByBranchId(branchId.Value).Select(t => new SelectListItem
                {
                    Text = t.TrackName,
                    Value = t.TrackId.ToString()
                }).ToList();
            }
            else
            {
                ViewBag.Tracks = new List<SelectListItem>();
            }
        }

        // Ajax endpoint to get tracks by branch id
        [HttpGet]
        public JsonResult GetTracksByBranch(int branchId)
        {
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(branchId)
                .Select(t => new { value = t.TrackId.ToString(), text = t.TrackName })
                .ToList();

            return Json(tracks);
        }
    }
}
