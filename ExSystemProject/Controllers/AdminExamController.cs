using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminExamController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminExamController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminExam
        //public IActionResult Index(int? courseId = null)
        //{
        //    var userId = GetCurrentUserId();

        //    List<Exam> exams;

        //    if (courseId.HasValue)
        //    {
        //        // Get exams for a specific course
        //        exams = _unitOfWork.courseRepo.GetExamsByCourseId(courseId.Value);
        //        ViewBag.CourseId = courseId;
        //        ViewBag.CourseName = _unitOfWork.courseRepo.GetCourseById(courseId.Value)?.CrsName;
        //    }
        //    else
        //    {
        //        // Get all exams
        //        exams = _unitOfWork.examRepo.GetAllExams();
        //    }

        //    var examDTOs = _mapper.Map<List<ExamDTO>>(exams);
        //    return View(examDTOs);
        //}
        public IActionResult Index(int? courseId = null, bool? isActive = null, int? insId = null, int pageNumber = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();

            // Get exams with filters
            var query = _unitOfWork.examRepo.GetAllExams(isActive, courseId, insId);

            // Count total exams for pagination
            var totalExams = query.Count();

            // Apply pagination
            var paginatedExams = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var examDTOs = _mapper.Map<List<ExamDTO>>(paginatedExams);

            // Set ViewBag properties for filters and pagination
            ViewBag.CourseId = courseId;
            if (courseId.HasValue)
            {
                ViewBag.CourseName = _unitOfWork.courseRepo.GetCourseById(courseId.Value)?.CrsName;
            }
            ViewBag.IsActive = isActive;
            ViewBag.InsId = insId;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalExams / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalExams = totalExams;

            // Populate filter dropdowns
            ViewBag.Courses = new SelectList(
                _unitOfWork.courseRepo.GetAllCourses(isActive: null, branchId: null, trackId: null),
                "CrsId",
                "CrsName",
                courseId
            );

            ViewBag.Instructors = new SelectList(
                _unitOfWork.instructorRepo.getAll(),
                "InsId",
                "User.Username",
                insId
            );

            return View(examDTOs);
        }
        // GET: AdminExam/Details/5
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();

            var exam = _unitOfWork.examRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            // Get questions for this exam
            var questions = _unitOfWork.examRepo.GetQuestionsByExamId(id);

            var examDTO = _mapper.Map<ExamDTO>(exam);
            examDTO.Questions = _mapper.Map<List<QuestionBankDTO>>(questions);

            return View(examDTO);
        }

        // GET: AdminExam/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();

            // Get all branches for the dropdown
            ViewBag.Branches = new SelectList(
                _unitOfWork.branchRepo.getAll(),
                "BranchId",
                "BranchName"
            );

            // Pre-populate empty selects for tracks, courses, and instructors
            ViewBag.Tracks = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Courses = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Instructors = new SelectList(Enumerable.Empty<SelectListItem>());

            // Return a model with default values
            return View(new ExamDTO
            {
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                TotalMarks = 100,
                PassedGrade = 60,
                Isactive = true
            });
        }

        // POST: AdminExam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExamDTO examDTO)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if an exam with the same name already exists for this course
                    var existingExams = _unitOfWork.examRepo.GetAllExams(null, examDTO.CrsId, null)
                        .Where(e => e.ExamName.ToLower() == examDTO.ExamName.ToLower())
                        .ToList();

                    if (existingExams.Any())
                    {
                        ModelState.AddModelError("ExamName", "An exam with this name already exists for the selected course.");
                        PopulateDropdowns(examDTO);
                        return View(examDTO);
                    }

                    var exam = _mapper.Map<Exam>(examDTO);

                    // Create blank exam
                    int examId = _unitOfWork.examRepo.CreateBlankExam(exam);

                    TempData["Success"] = true;
                    TempData["Message"] = $"Exam '{examDTO.ExamName}' has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = examId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating exam: {ex.Message}");
                }
            }

            PopulateDropdowns(examDTO);
            return View(examDTO);
        }

        // GET: AdminExam/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();

            var exam = _unitOfWork.examRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            var examDTO = _mapper.Map<ExamDTO>(exam);

            // Get branch and track info from the exam
            int? branchId = null;
            int? trackId = null;

            // First check if instructor exists and has track/branch info
            if (exam.InsId.HasValue)
            {
                var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(exam.InsId.Value);
                if (instructor != null && instructor.Track != null)
                {
                    trackId = instructor.TrackId;
                    branchId = instructor.Track.BranchId;
                }
            }

            // Make sure we have access to ViewBag.SelectedBranchId and ViewBag.SelectedTrackId for the view
            ViewBag.SelectedBranchId = branchId;
            ViewBag.SelectedTrackId = trackId;

            Console.WriteLine($"Selected Branch: {branchId}, Selected Track: {trackId}");

            // Populate all branches
            ViewBag.Branches = new SelectList(
                _unitOfWork.branchRepo.getAll(),
                "BranchId",
                "BranchName",
                branchId
            );

            // Populate tracks for the selected branch if we have one
            if (branchId.HasValue)
            {
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(branchId.Value);
                ViewBag.Tracks = new SelectList(
                    tracks,
                    "TrackId",
                    "TrackName",
                    trackId
                );

                Console.WriteLine($"Loading {tracks.Count} tracks for branch {branchId}");

                // Get courses for this track
                var courses = _unitOfWork.courseRepo.GetAllCourses(true, branchId, trackId).ToList();
                ViewBag.Courses = new SelectList(
                    courses,
                    "CrsId",
                    "CrsName",
                    exam.CrsId
                );

                Console.WriteLine($"Loading {courses.Count} courses for track {trackId}");

                // Get instructors for this track
                var instructors = trackId.HasValue
                    ? _unitOfWork.instructorRepo.GetInstructorsByTrackId(trackId.Value)
                    : new List<Instructor>();

                ViewBag.Instructors = new SelectList(
                    instructors,
                    "InsId",
                    "User.Username",
                    exam.InsId
                );

                Console.WriteLine($"Loading {instructors.Count} instructors for track {trackId}");
            }
            else
            {
                // Empty selects if no branch/track identified
                ViewBag.Tracks = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Courses = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Instructors = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View(examDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ExamDTO examDTO, bool? Isactive = null)
        {
            var userId = GetCurrentUserId();

            if (id != examDTO.ExamId)
                return NotFound();

            // Explicitly set Isactive property based on checkbox
            examDTO.Isactive = Isactive.HasValue && Isactive.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if an exam with the same name exists (other than this one)
                    var existingExams = _unitOfWork.examRepo.GetAllExams(null, examDTO.CrsId, null)
                        .Where(e => e.ExamName.ToLower() == examDTO.ExamName.ToLower() && e.ExamId != id)
                        .ToList();

                    if (existingExams.Any())
                    {
                        ModelState.AddModelError("ExamName", "Another exam with this name already exists for the selected course.");
                        PopulateDropdowns(examDTO);
                        return View(examDTO);
                    }

                    // Get the existing exam to preserve values that aren't in the form
                    var existingExam = _unitOfWork.examRepo.GetExamById(id);
                    if (existingExam == null)
                        return NotFound();

                    // Log the status value before update
                    Console.WriteLine($"Existing exam isActive: {existingExam.Isactive}, Form value: {Isactive}, examDTO value: {examDTO.Isactive}");

                    // Update the exam mapping carefully
                    var exam = _mapper.Map<Exam>(examDTO);

                    // Log the status value before update
                    Console.WriteLine($"Updating exam {id}: Status before update = {existingExam.Isactive}, Status after update = {exam.Isactive}");

                    // Update exam
                    _unitOfWork.examRepo.UpdateExam(exam);

                    TempData["Success"] = true;
                    TempData["Message"] = $"Exam '{examDTO.ExamName}' has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating exam: {ex.Message}");
                }
            }

            PopulateDropdowns(examDTO);
            return View(examDTO);
        }



        private void PopulateDropdowns(ExamDTO examDTO)
        {
            // Get all branches
            ViewBag.Branches = new SelectList(
                _unitOfWork.branchRepo.getAll(),
                "BranchId",
                "BranchName"
            );

            // If we have branch and track IDs, populate dependent dropdowns
            int? branchId = Request.Form["BranchId"].ToString().AsNullableInt();
            int? trackId = Request.Form["TrackId"].ToString().AsNullableInt();

            if (branchId.HasValue)
            {
                // Get tracks for this branch
                ViewBag.Tracks = new SelectList(
                    _unitOfWork.trackRepo.GetTracksByBranchId(branchId.Value),
                    "TrackId",
                    "TrackName",
                    trackId
                );

                if (trackId.HasValue)
                {
                    // Get courses for this track
                    var courses = _unitOfWork.courseRepo.GetAllCourses(true, branchId, trackId);
                    ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);

                    // Get instructors for this track
                    var instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackId(trackId.Value);
                    ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);
                }
                else
                {
                    ViewBag.Courses = new SelectList(Enumerable.Empty<SelectListItem>());
                    ViewBag.Instructors = new SelectList(Enumerable.Empty<SelectListItem>());
                }
            }
            else
            {
                ViewBag.Tracks = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Courses = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Instructors = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        // GET: /AdminExam/GetTracksByBranch
        [HttpGet]
        public JsonResult GetTracksByBranch(int branchId)
        {
            try
            {
                var tracks = _unitOfWork.trackRepo.GetTracksByBranchId(branchId);
                var trackList = tracks.Select(t => new { trackId = t.TrackId, trackName = t.TrackName }).ToList();
                return Json(trackList);
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new List<object>());
            }
        }

        // GET: /AdminExam/GetCoursesByTrack
        [HttpGet]
        public JsonResult GetCoursesByTrack(int trackId)
        {
            try
            {
                // Get track first to find branch ID
                var track = _unitOfWork.trackRepo.getById(trackId);
                if (track == null)
                    return Json(new List<object>());

                var courses = _unitOfWork.courseRepo.GetAllCourses(true, track.BranchId, trackId).ToList();
                var courseList = courses.Select(c => new { crsId = c.CrsId, crsName = c.CrsName }).ToList();
                return Json(courseList);
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new List<object>());
            }
        }

        // GET: /AdminExam/GetInstructorsByTrack
        [HttpGet]
        public JsonResult GetInstructorsByTrack(int trackId)
        {
            try
            {
                var instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackId(trackId);
                var instructorList = instructors.Select(i => new { insId = i.InsId, username = i.User?.Username }).ToList();
                return Json(instructorList);
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new List<object>());
            }
        }

        // GET: AdminExam/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

            var exam = _unitOfWork.examRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            var examDTO = _mapper.Map<ExamDTO>(exam);
            return View(examDTO);
        }

        // POST: AdminExam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var exam = _unitOfWork.examRepo.GetExamById(id);
            int? courseId = exam?.CrsId;

            // Delete exam
            _unitOfWork.examRepo.DeleteExam(id);

            if (courseId.HasValue)
                return RedirectToAction(nameof(Index), new { courseId = courseId });
            else
                return RedirectToAction(nameof(Index));
        }

        // GET: AdminExam/GenerateRandomExam
        public IActionResult GenerateRandomExam()
        {
            var userId = GetCurrentUserId();

            // Get all branches
            ViewBag.Branches = new SelectList(
                _unitOfWork.branchRepo.getAll(),
                "BranchId",
                "BranchName"
            );

            // Empty dropdowns until branch/track are selected
            ViewBag.Tracks = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Courses = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Instructors = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(new ExamDTO
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                TotalMarks = 100,
                PassedGrade = 60,
                Isactive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateRandomExam(ExamDTO examDTO, int mcqCount = 5, int tfCount = 5)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    // Generate random exam using the counts from the form
                    int examId = _unitOfWork.examRepo.GenerateRandomExam(
                        examDTO.ExamName,
                        examDTO.CrsId.Value,
                        examDTO.InsId.Value,
                        mcqCount,
                        tfCount,
                        examDTO.StartTime.Value,
                        examDTO.EndTime.Value
                    );

                    TempData["Success"] = true;
                    TempData["Message"] = "Random exam generated successfully!";
                    return RedirectToAction(nameof(Details), new { id = examId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            PopulateDropdowns(examDTO);
            return View(examDTO);
        }
    }

    // Extension method to parse nullable int
    public static class StringExtensions
    {
        public static int? AsNullableInt(this string str)
        {
            if (int.TryParse(str, out int result))
                return result;
            return null;
        }
    }
}
