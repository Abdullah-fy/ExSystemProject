using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminInstructorController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminInstructorController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminInstructor
        public IActionResult Index(int? branchId = null, int? trackId = null, bool? activeOnly = null)
        {
            var userId = GetCurrentUserId();

            List<Instructor> instructors;

            if (branchId.HasValue && trackId.HasValue)
            {
                Track track = _unitOfWork.trackRepo.getById(trackId.Value);
                Branch branch = _unitOfWork.branchRepo.getById(branchId.Value);

                instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackWithBranch(trackId.Value, activeOnly);

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch?.BranchName;
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = track?.TrackName;
            }
            else if (branchId.HasValue)
            {
                instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(branchId.Value, activeOnly);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = _unitOfWork.branchRepo.getById(branchId.Value)?.BranchName;
            }
            else if (trackId.HasValue)
            {
                instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackId(trackId.Value, activeOnly);
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = _unitOfWork.trackRepo.getById(trackId.Value)?.TrackName;
            }
            else
            {
                instructors = _unitOfWork.instructorRepo.GetAllInstructorsWithBranch(activeOnly);
            }

            var branches = _unitOfWork.branchRepo.getAll();
            var tracks = _unitOfWork.trackRepo.GetDistictTracks();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", branchId);
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", trackId);
            ViewBag.ActiveOnly = activeOnly;

            var instructorDTOs = _mapper.Map<List<InstructorDTO>>(instructors);
            return View(instructorDTOs);
        }


        // GET: AdminInstructor/Details/5
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();

            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            if (instructor == null)
            {
                return NotFound();
            }

            var courses = _unitOfWork.instructorRepo.GetInstructorCourses(id);

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            instructorDTO.AssignedCourses = _mapper.Map<List<CourseDTO>>(courses);

            return View(instructorDTO);
        }

        // GET: AdminInstructor/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();

            PopulateDropDowns();
            return View();
        }

        // POST: AdminInstructor/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InstructorDTO instructorDTO, string Password)
        {
            var userId = GetCurrentUserId();

            try
            {
                if (string.IsNullOrEmpty(instructorDTO.Username) ||
                    string.IsNullOrEmpty(instructorDTO.Email) ||
                    string.IsNullOrEmpty(instructorDTO.Gender) ||
                    string.IsNullOrEmpty(Password) ||
                    !instructorDTO.TrackId.HasValue ||
                    !instructorDTO.Salary.HasValue)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields";
                    PopulateDropDowns();
                    return View(instructorDTO);
                }

                _unitOfWork.instructorRepo.CreateInstructor(
                    instructorDTO.Username,
                    instructorDTO.Email,
                    instructorDTO.Gender,
                    Password, 
                    instructorDTO.Salary ?? 0,
                    instructorDTO.TrackId ?? 0
                );

                TempData["SuccessMessage"] = "Instructor created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating instructor: " + ex.Message;
                PopulateDropDowns();
                return View(instructorDTO);
            }
        }




        // GET: AdminInstructor/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();

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
            var userId = GetCurrentUserId();

            if (id != instructorDTO.InsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool isActive = instructorDTO.Isactive ?? true;
                    Console.WriteLine($"IsActive value being used: {isActive}");

                    _unitOfWork.instructorRepo.UpdateInstructor(
                        instructorDTO.InsId,
                        instructorDTO.Username,
                        instructorDTO.Email,
                        instructorDTO.Gender,
                        instructorDTO.Salary ?? 0,
                        instructorDTO.TrackId ?? 0,
                        isActive  
                    );

                    TempData["SuccessMessage"] = "Instructor updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating instructor: {ex.Message}");
                    Console.WriteLine($"Error updating instructor: {ex.Message}");
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            PopulateDropDowns(instructorDTO.BranchId);
            return View(instructorDTO);
        }

        // GET: AdminInstructor/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

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
            var userId = GetCurrentUserId();

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
            var userId = GetCurrentUserId();

            var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(id);

            if (instructor == null)
            {
                return NotFound();
            }

            var coursesData = _unitOfWork.instructorRepo.GetInstructorCoursesWithStudentCount(id);

            var instructorDTO = _mapper.Map<InstructorDTO>(instructor);

            ViewBag.InstructorData = instructorDTO;
            ViewBag.CoursesData = coursesData;

            return View();
        }

        private void PopulateDropDowns(int? branchId = null)
        {
            ViewBag.Branches = _unitOfWork.branchRepo.getAll().Select(b => new SelectListItem
            {
                Text = b.BranchName,
                Value = b.BranchId.ToString()
            }).ToList();

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
