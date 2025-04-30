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
    public class BranchManagerInstructorController : BranchManagerBaseController
    {
        public BranchManagerInstructorController(UnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        // GET: BranchManagerInstructor
        public IActionResult Index(int? trackId = null, bool? active = true)
        {
            ViewData["Title"] = "Instructor Management";

            // Get instructors for this branch
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, null);

            // Filter by track if specified
            if (trackId.HasValue)
            {
                instructors = instructors.Where(i => i.TrackId == trackId.Value).ToList();
            }

            // Filter by active status if specified
            if (active.HasValue)
            {
                instructors = instructors.Where(i => i.Isactive == active.Value).ToList();
            }

            // Get tracks for filtering
            ViewBag.Tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId).Select(t => new SelectListItem
            {
                Text = t.TrackName,
                Value = t.TrackId.ToString(),
                Selected = trackId.HasValue && t.TrackId == trackId.Value
            }).ToList();

            ViewBag.SelectedTrack = trackId;
            ViewBag.ActiveFilter = active;

            return View(instructors);
        }

        // GET: BranchManagerInstructor/Details/5
        public IActionResult Details(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get instructor courses
            var courses = _unitOfWork.instructorRepo.GetInstructorCourses(id);
            ViewBag.Courses = courses;

            ViewData["Title"] = $"Instructor: {instructor.User?.Username}";
            return View(instructor);
        }

        // GET: BranchManagerInstructor/Create
        public IActionResult Create()
        {
            // Get tracks for this branch
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                .Where(t => t.IsActive == true)
                .ToList();

            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName");
            ViewData["Title"] = "Add New Instructor";

            return View(new InstructorDTO { BranchId = CurrentBranchId, BranchName = CurrentBranchName });
        }

        // POST: BranchManagerInstructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InstructorDTO instructorDTO, string password)
        {
            // Always ensure instructor is assigned to a track in this branch
            var track = _unitOfWork.trackRepo.getById(instructorDTO.TrackId ?? 0);
            if (track == null || track.BranchId != CurrentBranchId)
            {
                ModelState.AddModelError("TrackId", "Please select a valid track from this branch");
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(password))
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                }

                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);
                ViewData["Title"] = "Add New Instructor";

                return View(instructorDTO);
            }

            try
            {
                // Create instructor with stored procedure
                _unitOfWork.instructorRepo.CreateInstructor(
                    instructorDTO.Username,
                    instructorDTO.Email,
                    instructorDTO.Gender,
                    password,
                    instructorDTO.Salary ?? 0,
                    instructorDTO.TrackId ?? 0
                );

                TempData["Success"] = "Instructor created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating instructor: {ex.Message}");

                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);
                ViewData["Title"] = "Add New Instructor";

                return View(instructorDTO);
            }
        }

        // GET: BranchManagerInstructor/Edit/5
        public IActionResult Edit(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get tracks for this branch
            var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                .Where(t => t.IsActive == true)
                .ToList();

            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructor.TrackId);

            // Map instructor to DTO
            var instructorDTO = new InstructorDTO
            {
                InsId = instructor.InsId,
                Username = instructor.User?.Username,
                Email = instructor.User?.Email,
                Gender = instructor.User?.Gender,
                Salary = instructor.Salary,
                TrackId = instructor.TrackId,
                TrackName = instructor.Track?.TrackName ?? string.Empty,
                BranchId = instructor.Track?.BranchId,
                BranchName = instructor.Track?.Branch?.BranchName ?? string.Empty,
                Isactive = instructor.Isactive,
                ImageUrl = instructor.User?.Img ?? "default.jpg" // Use default if null
            };

            ViewData["Title"] = $"Edit Instructor: {instructorDTO.Username}";
            return View(instructorDTO);
        }

        // POST: BranchManagerInstructor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InstructorDTO instructorDTO)
        {
            if (id != instructorDTO.InsId)
            {
                return NotFound();
            }

            // Ensure instructor is assigned to a track in this branch
            var track = _unitOfWork.trackRepo.getById(instructorDTO.TrackId ?? 0);
            if (track == null || track.BranchId != CurrentBranchId)
            {
                ModelState.AddModelError("TrackId", "Please select a valid track from this branch");
            }

            // Set default image URL if not provided
            instructorDTO.ImageUrl ??= "default.jpg";

            if (!ModelState.IsValid)
            {
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);
                ViewData["Title"] = $"Edit Instructor: {instructorDTO.Username}";

                return View(instructorDTO);
            }

            try
            {
                // Update instructor with stored procedure
                _unitOfWork.instructorRepo.UpdateInstructor(
                    instructorDTO.InsId,
                    instructorDTO.Username,
                    instructorDTO.Email,
                    instructorDTO.Gender,
                    instructorDTO.Salary ?? 0,
                    instructorDTO.TrackId ?? 0,
                    instructorDTO.Isactive ?? true
                );

                TempData["Success"] = "Instructor updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating instructor: {ex.Message}");

                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(CurrentBranchId)
                    .Where(t => t.IsActive == true)
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", instructorDTO.TrackId);
                ViewData["Title"] = $"Edit Instructor: {instructorDTO.Username}";

                return View(instructorDTO);
            }
        }

        // GET: BranchManagerInstructor/Delete/5
        public IActionResult Delete(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Instructor: {instructor.User?.Username}";
            return View(instructor);
        }

        // POST: BranchManagerInstructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.instructorRepo.DeleteInstructor(id);
                TempData["Success"] = "Instructor deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting instructor: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: BranchManagerInstructor/Courses/5
        public IActionResult Courses(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get instructor courses
            var coursesData = _unitOfWork.instructorRepo.GetInstructorCoursesWithStudentCount(id);

            ViewBag.Instructor = instructor;
            ViewData["Title"] = $"Courses for {instructor.User?.Username}";

            return View(coursesData);
        }

        // GET: BranchManagerInstructor/AssignCourse/5
        public IActionResult AssignCourse(int id)
        {
            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get unassigned courses
            var availableCourses = _unitOfWork.courseRepo.GetAllCourses(true)
                .Where(c => c.InsId == null || c.InsId == id)
                .ToList();

            ViewBag.Instructor = instructor;
            ViewBag.Courses = new SelectList(availableCourses, "CrsId", "CrsName");
            ViewData["Title"] = $"Assign Course to {instructor.User?.Username}";

            return View();
        }

        // POST: BranchManagerInstructor/AssignCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignCourse(int instructorId, int courseId)
        {
            var instructor = _unitOfWork.instructorRepo.getById(instructorId);
            var course = _unitOfWork.courseRepo.getById(courseId);

            // Verify instructor exists and belongs to this branch
            if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            if (course == null)
            {
                return NotFound("Course not found");
            }

            try
            {
                // Assign course to instructor
                course.InsId = instructorId;
                _unitOfWork.courseRepo.update(course);
                _unitOfWork.save();

                TempData["Success"] = "Course assigned successfully";
                return RedirectToAction(nameof(Courses), new { id = instructorId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error assigning course: {ex.Message}";
                return RedirectToAction(nameof(AssignCourse), new { id = instructorId });
            }
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            try
            {
                var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

                // Verify instructor exists and belongs to this branch
                if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
                {
                    return Json(new { success = false, message = "Instructor not found or access denied" });
                }

                // Toggle active status
                bool newStatus = !(instructor.Isactive ?? true);

                _unitOfWork.instructorRepo.UpdateInstructor(
                    instructor.InsId,
                    instructor.User.Username,
                    instructor.User.Email,
                    instructor.User.Gender,
                    instructor.Salary ?? 0,
                    instructor.TrackId ?? 0,
                    newStatus
                );

                string statusText = newStatus ? "activated" : "deactivated";
                return Json(new
                {
                    success = true,
                    message = $"Instructor {statusText} successfully",
                    isActive = newStatus
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
