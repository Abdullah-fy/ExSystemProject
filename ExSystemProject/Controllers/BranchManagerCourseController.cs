using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class BranchManagerCourseController : BranchManagerBaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BranchManagerCourseController(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : base(unitOfWork)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: BranchManagerCourse
        public IActionResult Index(bool? active = true)
        {
            ViewData["Title"] = "Course Management";

            // Get all courses where the instructor belongs to the branch manager's branch
            var courses = _unitOfWork.courseRepo.GetAllCourses(active)
                .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                .ToList();

            return View(courses);
        }

        // GET: BranchManagerCourse/Details/5
        public IActionResult Details(int id)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(id);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get related data
            var topics = _unitOfWork.topicRepo.GetTopicsByCourseId(id, null);
            var exams = _unitOfWork.courseRepo.GetExamsByCourseId(id);

            ViewBag.Topics = topics;
            ViewBag.Exams = exams;

            ViewData["Title"] = $"Course: {course.CrsName}";
            return View(course);
        }

        // GET: BranchManagerCourse/Create
        public IActionResult Create()
        {
            // Get instructors from this branch only
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");
            ViewData["Title"] = "Create New Course";
            return View();
        }

        // POST: BranchManagerCourse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Course course)
        {
            if (ModelState.IsValid)
            {
                // Verify instructor belongs to this branch
                if (course.InsId.HasValue)
                {
                    var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(course.InsId.Value);
                    if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
                    {
                        ModelState.AddModelError("InsId", "Selected instructor does not belong to your branch");
                        var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
                        ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");
                        return View(course);
                    }
                }

                try
                {
                    // Set default values
                    course.Isactive = true;
                    // No poster handling at all

                    _unitOfWork.courseRepo.CreateCourse(course);
                    TempData["Success"] = "Course created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating course: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay form
            var branchInstructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
            ViewBag.Instructors = new SelectList(branchInstructors, "InsId", "User.Username");
            return View(course);
        }


        // GET: BranchManagerCourse/Edit/5
        public IActionResult Edit(int id)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(id);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get instructors from this branch only
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", course.InsId);

            ViewData["Title"] = $"Edit Course: {course.CrsName}";
            return View(course);
        }

        // POST: BranchManagerCourse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Course course)
        {
            if (id != course.CrsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verify instructor belongs to this branch
                if (course.InsId.HasValue)
                {
                    var instructor = _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(course.InsId.Value);
                    if (instructor == null || instructor.Track?.BranchId != CurrentBranchId)
                    {
                        ModelState.AddModelError("InsId", "Selected instructor does not belong to your branch");
                        var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
                        ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");
                        return View(course);
                    }
                }

                try
                {
                    // Get the existing course to preserve any fields not in the form
                    var existingCourse = _unitOfWork.courseRepo.GetCourseById(id);
                    if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    // Keep the poster from existing course
                    course.Poster = existingCourse.Poster;

                    // Make sure isActive is not null
                    course.Isactive = course.Isactive ?? existingCourse.Isactive ?? true;

                    _unitOfWork.courseRepo.UpdateCourse(course);
                    TempData["Success"] = "Course updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating course: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay form
            var branchInstructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
            ViewBag.Instructors = new SelectList(branchInstructors, "InsId", "User.Username", course.InsId);
            return View(course);
        }


        // GET: BranchManagerCourse/Delete/5
        public IActionResult Delete(int id)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(id);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Course: {course.CrsName}";
            return View(course);
        }

        // POST: BranchManagerCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(id);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.courseRepo.DeleteCourse(id);
                TempData["Success"] = "Course deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting course: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: BranchManagerCourse/Topics/5
        public IActionResult Topics(int id)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(id);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get topics for this course
            var topics = _unitOfWork.topicRepo.GetTopicsByCourseId(id, null);

            ViewBag.Course = course;
            ViewBag.Topics = topics;

            ViewData["Title"] = $"Topics for {course.CrsName}";
            return View();
        }

        // GET: BranchManagerCourse/AddTopic/5
        public IActionResult AddTopic(int id)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(id);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewBag.Course = course;
            ViewData["Title"] = $"Add Topic to {course.CrsName}";

            return View(new Topic { CrsId = id });
        }

        // POST: BranchManagerCourse/AddTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTopic(Topic topic)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(topic.CrsId ?? 0);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.topicRepo.CreateTopic(
                        topic.TopicName ?? string.Empty,
                        topic.Descrtption ?? string.Empty,
                        topic.CrsId ?? 0
                    );
                    TempData["Success"] = "Topic added successfully";
                    return RedirectToAction(nameof(Topics), new { id = topic.CrsId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding topic: {ex.Message}");
                }
            }

            ViewBag.Course = course;
            ViewData["Title"] = $"Add Topic to {course.CrsName}";
            return View(topic);
        }

        // GET: BranchManagerCourse/EditTopic/5
        public IActionResult EditTopic(int id)
        {
            var topic = _unitOfWork.topicRepo.GetTopicById(id);

            // Verify topic exists and course belongs to this branch
            if (topic == null)
            {
                return NotFound();
            }

            var course = _unitOfWork.courseRepo.GetCourseById(topic.CrsId ?? 0);
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewBag.Course = course;
            ViewData["Title"] = $"Edit Topic for {course.CrsName}";

            return View(topic);
        }

        // POST: BranchManagerCourse/EditTopic/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTopic(int id, Topic topic)
        {
            if (id != topic.TopicId)
            {
                return NotFound();
            }

            var course = _unitOfWork.courseRepo.GetCourseById(topic.CrsId ?? 0);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.topicRepo.UpdateTopic(
                        topic.TopicId,
                        topic.TopicName ?? string.Empty,
                        topic.Descrtption ?? string.Empty,
                        topic.CrsId ?? 0,
                        topic.Isactive ?? true
                    );
                    TempData["Success"] = "Topic updated successfully";
                    return RedirectToAction(nameof(Topics), new { id = topic.CrsId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating topic: {ex.Message}");
                }
            }

            ViewBag.Course = course;
            ViewData["Title"] = $"Edit Topic for {course.CrsName}";
            return View(topic);
        }

        // GET: BranchManagerCourse/DeleteTopic/5
        public IActionResult DeleteTopic(int id)
        {
            var topic = _unitOfWork.topicRepo.GetTopicById(id);

            // Verify topic exists and course belongs to this branch
            if (topic == null)
            {
                return NotFound();
            }

            var course = _unitOfWork.courseRepo.GetCourseById(topic.CrsId ?? 0);
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewBag.Course = course;
            ViewData["Title"] = $"Delete Topic from {course.CrsName}";

            return View(topic);
        }

        // POST: BranchManagerCourse/DeleteTopic/5
        [HttpPost, ActionName("DeleteTopic")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTopicConfirmed(int id)
        {
            var topic = _unitOfWork.topicRepo.GetTopicById(id);

            // Verify topic exists
            if (topic == null)
            {
                return NotFound();
            }

            var courseId = topic.CrsId;

            // Verify course exists and belongs to this branch
            var course = _unitOfWork.courseRepo.GetCourseById(courseId ?? 0);
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.topicRepo.DeleteTopic(id);
                TempData["Success"] = "Topic deleted successfully";
                return RedirectToAction(nameof(Topics), new { id = courseId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting topic: {ex.Message}";
                return RedirectToAction(nameof(DeleteTopic), new { id });
            }
        }
       
        [HttpPost]
        public IActionResult ToggleTopicStatus(int id)
        {
            try
            {
                var updatedTopic = _unitOfWork.topicRepo.ToggleTopicStatus(id);

                string statusText = updatedTopic.Isactive == true ? "activated" : "deactivated";
                return Json(new
                {
                    success = true,
                    message = $"Topic {statusText} successfully",
                    isActive = updatedTopic.Isactive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
