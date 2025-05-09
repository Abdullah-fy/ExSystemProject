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
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminCourseController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminCourseController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminCourse
        //public IActionResult Index()
        //{
        //    var userId = GetCurrentUserId();

        //    // Pass null to get both active and inactive courses
        //    var courses = _unitOfWork.courseRepo.GetAllCourses(null);
        //    var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);
        //    return View(courseDTOs);
        //}
        public IActionResult Index(bool? isActive = null, int? branchId = null, int? trackId = null, int pageNumber = 1, int pageSize = 10)
        {
            var courses = _unitOfWork.courseRepo.GetAllCourses(isActive, branchId, trackId);
            var paginatedCourses = courses.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var courseDTOs = _mapper.Map<List<CourseNewDTO>>(paginatedCourses);

            var branches = _unitOfWork.branchRepo.GetAllActive();
            var tracks = branchId.HasValue ? _unitOfWork.trackRepo.GetActiveTracksByBranchId(branchId.Value) : new List<Track>();

            var viewModel = new CourseListViewModel
            {
                Courses = courseDTOs,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(courses.Count() / (double)pageSize),
                PageSize = pageSize,
                IsActive = isActive,
                BranchId = branchId,
                TrackId = trackId,
                Branches = new SelectList(branches, "BranchId", "BranchName", branchId),
                Tracks = new SelectList(tracks, "TrackId", "TrackName", trackId)
            };

            return View(viewModel);
        }
        // GET: AdminCourse/Details/5
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();

            // Using the enhanced repository method to get course by id
            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);

            // Get additional course information
            var topics = _unitOfWork.courseRepo.GetCourseTopics(id);
            var exams = _unitOfWork.courseRepo.GetExamsByCourseId(id);

            ViewBag.Topics = topics;
            ViewBag.Exams = exams;

            return View(courseDTO);
        }

        // GET: AdminCourse/Create
        public IActionResult Create()
        {
            var branches = _unitOfWork.branchRepo.GetAllActive();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

            // Tracks will be loaded via AJAX when branch is selected
            ViewBag.Tracks = new SelectList(new List<Track>(), "TrackId", "TrackName");

            // Instructors will be loaded via AJAX when track is selected
            ViewBag.Instructors = new SelectList(new List<Instructor>(), "InsId", "User.Username");

            return View(new CourseDTO { Isactive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseDTO courseDTO)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns
                var branches = _unitOfWork.branchRepo.GetAllActive();
                ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", courseDTO.BranchId);

                if (courseDTO.BranchId.HasValue)
                {
                    var tracks = _unitOfWork.trackRepo.GetActiveTracksByBranchId(courseDTO.BranchId.Value);
                    ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", courseDTO.TrackId);

                    if (courseDTO.TrackId.HasValue)
                    {
                        var instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackWithBranch(courseDTO.TrackId.Value, true);
                        ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", courseDTO.InsId);
                    }
                }

                return View(courseDTO);
            }

            // Save course logic here
            var course = _mapper.Map<Course>(courseDTO);
            _unitOfWork.courseRepo.CreateCourse(course);

            TempData["Success"] = true;
            TempData["Message"] = $"Course '{courseDTO.CrsName}' has been created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetTracksByBranch(int id)
        {
            var tracks = _unitOfWork.trackRepo.GetActiveTracksByBranchId(id);
            return Json(tracks.Select(t => new { trackId = t.TrackId, trackName = t.TrackName }).ToList());
        }

        [HttpGet]
        public IActionResult GetInstructorsByTrack(int id)
        {
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByTrackWithBranch(id, true);
            return Json(instructors.Select(i => new { insId = i.InsId, insName = i.User?.Username }).ToList());
        }



        // GET: AdminCourse/Edit/5
        public IActionResult Edit(int id)
        {
            // Using the enhanced repository method to get course by id
            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);

            // Get branches for dropdown
            var branches = _unitOfWork.branchRepo.GetAllActive();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", courseDTO.BranchId);

            // Get tracks for the selected branch
            if (courseDTO.BranchId.HasValue)
            {
                var tracks = _unitOfWork.trackRepo.GetActiveTracksByBranchId(courseDTO.BranchId.Value);
                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", courseDTO.TrackId);
            }
            else
            {
                ViewBag.Tracks = new SelectList(new List<Track>(), "TrackId", "TrackName");
            }

            return View(courseDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CourseDTO courseDTO)
        {
            if (id != courseDTO.CrsId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Get current course to compare isactive state
                    var currentCourse = _unitOfWork.courseRepo.GetCourseById(id);
                    var wasActive = currentCourse?.Isactive ?? true;

                    // Make sure Isactive is never null before updating
                    if (courseDTO.Isactive == null)
                    {
                        courseDTO.Isactive = wasActive;
                    }

                    var course = _mapper.Map<Course>(courseDTO);

                    // Using the enhanced repository method to update a course
                    _unitOfWork.courseRepo.UpdateCourse(course);

                    TempData["Success"] = true;

                    // Custom message based on active status change
                    if (wasActive == true && courseDTO.Isactive == false)
                    {
                        TempData["Message"] = $"Course '{courseDTO.CrsName}' has been deactivated successfully.";
                    }
                    else if (wasActive == false && courseDTO.Isactive == true)
                    {
                        TempData["Message"] = $"Course '{courseDTO.CrsName}' has been activated successfully.";
                    }
                    else
                    {
                        TempData["Message"] = $"Course '{courseDTO.CrsName}' has been updated successfully.";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the exception
                    System.Diagnostics.Debug.WriteLine($"Error updating course: {ex.Message}");
                    ModelState.AddModelError(string.Empty, $"Error updating course: {ex.Message}");
                }
            }

            // If we got here, something failed; redisplay form
            var branches = _unitOfWork.branchRepo.GetAllActive();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", courseDTO.BranchId);

            if (courseDTO.BranchId.HasValue)
            {
                var tracks = _unitOfWork.trackRepo.GetActiveTracksByBranchId(courseDTO.BranchId.Value);
                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", courseDTO.TrackId);
            }
            else
            {
                ViewBag.Tracks = new SelectList(new List<Track>(), "TrackId", "TrackName");
            }

            return View(courseDTO);
        }


        // GET: AdminCourse/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

            // Using the enhanced repository method to get course by id
            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);
            return View(courseDTO);
        }

        // POST: AdminCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                // Get the course to check if it exists
                var course = _unitOfWork.courseRepo.GetCourseById(id);
                if (course == null)
                {
                    return NotFound();
                }

                // Delete the course using the repository method
                _unitOfWork.courseRepo.DeleteCourse(id);

                TempData["Success"] = true;
                TempData["Message"] = "Course deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error deleting course: {ex.Message}");

                // Use TempData to display error message
                TempData["Error"] = true;
                TempData["Message"] = $"Error deleting course: {ex.Message}";

                // Redirect back to the delete confirmation page
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: AdminCourse/Topics/5
        public IActionResult Topics(int id)
        {
            var userId = GetCurrentUserId();

            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);

            // Get topics for this course
            var topics = _unitOfWork.topicRepo.GetTopicsByCourseId(id, null); // Get all topics (active and inactive)

            ViewBag.Course = courseDTO;
            ViewBag.Topics = topics;

            return View();
        }

        // GET: AdminCourse/AddTopic/5
        public IActionResult AddTopic(int id)
        {
            var userId = GetCurrentUserId();

            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);
            ViewBag.Course = courseDTO;

            return View(new TopicDTO { CrsId = id });
        }

        // POST: AdminCourse/AddTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTopic(TopicDTO topicDTO)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    var course = _unitOfWork.courseRepo.GetCourseById(topicDTO.CrsId ?? 0);
                    if (course == null)
                        return NotFound();

                    // Create the topic
                    _unitOfWork.topicRepo.CreateTopic(
                        topicDTO.TopicName ?? string.Empty,
                        topicDTO.Description ?? string.Empty,
                        topicDTO.CrsId ?? 0
                    );

                    TempData["Success"] = true;
                    TempData["Message"] = $"Topic '{topicDTO.TopicName}' added successfully to course '{course.CrsName}'.";

                    return RedirectToAction(nameof(Topics), new { id = topicDTO.CrsId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding topic: {ex.Message}");
                }
            }

            var courseDTO = _mapper.Map<CourseDTO>(_unitOfWork.courseRepo.GetCourseById(topicDTO.CrsId ?? 0));
            ViewBag.Course = courseDTO;

            return View(topicDTO);
        }

        // GET: AdminCourse/EditTopic/5
        public IActionResult EditTopic(int id)
        {
            var userId = GetCurrentUserId();

            var topic = _unitOfWork.topicRepo.GetTopicById(id);
            if (topic == null)
                return NotFound();

            var topicDTO = new TopicDTO
            {
                TopicId = topic.TopicId,
                TopicName = topic.TopicName,
                Description = topic.Descrtption,
                CrsId = topic.CrsId,
                IsActive = topic.Isactive,
                CourseName = topic.Crs?.CrsName
            };

            // Get course info for the view header
            var course = _unitOfWork.courseRepo.GetCourseById(topic.CrsId ?? 0);
            ViewBag.Course = _mapper.Map<CourseDTO>(course);

            return View(topicDTO);
        }

        // POST: AdminCourse/EditTopic/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTopic(int id, TopicDTO topicDTO)
        {
            var userId = GetCurrentUserId();

            if (id != topicDTO.TopicId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the topic
                    _unitOfWork.topicRepo.UpdateTopic(
                        topicDTO.TopicId ?? 0,
                        topicDTO.TopicName ?? string.Empty,
                        topicDTO.Description ?? string.Empty,
                        topicDTO.CrsId ?? 0,
                        topicDTO.IsActive ?? true
                    );

                    TempData["Success"] = true;
                    TempData["Message"] = $"Topic '{topicDTO.TopicName}' updated successfully.";

                    return RedirectToAction(nameof(Topics), new { id = topicDTO.CrsId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating topic: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay form
            var course = _unitOfWork.courseRepo.GetCourseById(topicDTO.CrsId ?? 0);
            ViewBag.Course = _mapper.Map<CourseDTO>(course);

            return View(topicDTO);
        }

        // GET: AdminCourse/DeleteTopic/5
        public IActionResult DeleteTopic(int id)
        {
            var userId = GetCurrentUserId();

            var topic = _unitOfWork.topicRepo.GetTopicById(id);
            if (topic == null)
                return NotFound();

            var topicDTO = new TopicDTO
            {
                TopicId = topic.TopicId,
                TopicName = topic.TopicName,
                Description = topic.Descrtption,
                CrsId = topic.CrsId,
                IsActive = topic.Isactive,
                CourseName = topic.Crs?.CrsName
            };

            // Get course info for the view header
            var course = _unitOfWork.courseRepo.GetCourseById(topic.CrsId ?? 0);
            ViewBag.Course = _mapper.Map<CourseDTO>(course);

            return View(topicDTO);
        }

        // POST: AdminCourse/DeleteTopic/5
        [HttpPost, ActionName("DeleteTopic")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTopicConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var topic = _unitOfWork.topicRepo.GetTopicById(id);
            if (topic == null)
                return NotFound();

            var courseId = topic.CrsId;

            try
            {
                _unitOfWork.topicRepo.DeleteTopic(id);

                TempData["Success"] = true;
                TempData["Message"] = "Topic deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = true;
                TempData["Message"] = $"Error deleting topic: {ex.Message}";
            }

            return RedirectToAction(nameof(Topics), new { id = courseId });
        }

        // POST: AdminCourse/ToggleTopicStatus/5
        [HttpPost]
        public IActionResult ToggleTopicStatus(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                // Get the topic
                var topic = _unitOfWork.topicRepo.GetTopicById(id);
                if (topic == null)
                    return Json(new { success = false, message = "Topic not found" });

                // Determine the new status (opposite of current)
                bool currentStatus = topic.Isactive ?? true;
                bool newStatus = !currentStatus;

                // Update the topic using stored procedure
                _unitOfWork.topicRepo.UpdateTopic(
                    topic.TopicId,
                    topic.TopicName ?? string.Empty,
                    topic.Descrtption ?? string.Empty,
                    topic.CrsId ?? 0,
                    newStatus
                );

                // Return success response with the new status
                string statusText = newStatus ? "activated" : "deactivated";
                return Json(new
                {
                    success = true,
                    message = $"Topic {statusText} successfully",
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
