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
    public class BranchManagerExamController : BranchManagerBaseController
    {
        public BranchManagerExamController(UnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        // GET: BranchManagerExam
        public IActionResult Index(int? courseId = null)
        {
            ViewData["Title"] = "Exam Management";

            List<Exam> exams;

            if (courseId.HasValue)
            {
                // Get course to verify it belongs to this branch
                var course = _unitOfWork.courseRepo.GetCourseById(courseId.Value);
                if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
                {
                    return NotFound();
                }

                // Get exams for the specified course
                exams = _unitOfWork.courseRepo.GetExamsByCourseId(courseId.Value);
                ViewBag.CourseId = courseId;
                ViewBag.CourseName = course.CrsName;
            }
            else
            {
                // Get all exams for this branch only
                exams = _unitOfWork.examRepo.GetAllExams()
                    .Where(e => e.Ins?.Track?.BranchId == CurrentBranchId)
                    .ToList();
            }

            return View(exams);
        }

        // GET: BranchManagerExam/Details/5
        public IActionResult Details(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get questions and results
            var questions = _unitOfWork.examRepo.GetQuestionsByExamId(id);
            var results = _unitOfWork.examRepo.GetExamResults(id);

            ViewBag.Questions = questions;
            ViewBag.Results = results;
            ViewBag.TotalStudents = results.Count();
            ViewBag.PassedStudents = results.Count(r => r.Score >= exam.PassedGrade);
            ViewBag.FailedStudents = results.Count(r => r.Score < exam.PassedGrade);

            ViewData["Title"] = $"Exam: {exam.ExamName}";
            return View(exam);
        }

        // GET: BranchManagerExam/Create
        public IActionResult Create(int? courseId = null)
        {
            // Get courses from this branch only
            var courses = _unitOfWork.courseRepo.GetAllCourses(true)
                .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                .ToList();

            // Get instructors from this branch only
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", courseId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");

            // Default values for new exam
            var exam = new Exam
            {
                CrsId = courseId,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                Isactive = true
            };

            ViewData["Title"] = "Create New Exam";
            return View(exam);
        }

        // POST: BranchManagerExam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                // Verify course and instructor belong to this branch
                var course = _unitOfWork.courseRepo.GetCourseById(exam.CrsId ?? 0);
                var instructor = exam.InsId.HasValue ? _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(exam.InsId.Value) : null;

                if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId ||
                    (instructor != null && instructor.Track?.BranchId != CurrentBranchId))
                {
                    ModelState.AddModelError("", "Selected course or instructor does not belong to your branch");

                    // Re-populate dropdowns
                    var courses = _unitOfWork.courseRepo.GetAllCourses(true)
                        .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                        .ToList();
                    var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

                    ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", exam.CrsId);
                    ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", exam.InsId);

                    return View(exam);
                }

                try
                {
                    // Create blank exam
                    int examId = _unitOfWork.examRepo.CreateBlankExam(exam);
                    TempData["Success"] = "Exam created successfully";
                    return RedirectToAction(nameof(Details), new { id = examId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating exam: {ex.Message}");
                }
            }

            // Re-populate dropdowns if we reach here due to validation failure
            var branchCourses = _unitOfWork.courseRepo.GetAllCourses(true)
                .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                .ToList();
            var branchInstructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

            ViewBag.Courses = new SelectList(branchCourses, "CrsId", "CrsName", exam.CrsId);
            ViewBag.Instructors = new SelectList(branchInstructors, "InsId", "User.Username", exam.InsId);

            ViewData["Title"] = "Create New Exam";
            return View(exam);
        }

        // GET: BranchManagerExam/Edit/5
        public IActionResult Edit(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get courses and instructors from this branch
            var courses = _unitOfWork.courseRepo.GetAllCourses(true)
                .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                .ToList();
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", exam.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", exam.InsId);

            ViewData["Title"] = $"Edit Exam: {exam.ExamName}";
            return View(exam);
        }

        // POST: BranchManagerExam/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Exam exam)
        {
            if (id != exam.ExamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verify course and instructor belong to this branch
                var course = _unitOfWork.courseRepo.GetCourseById(exam.CrsId ?? 0);
                var instructor = exam.InsId.HasValue ? _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(exam.InsId.Value) : null;

                if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId ||
                    (instructor != null && instructor.Track?.BranchId != CurrentBranchId))
                {
                    ModelState.AddModelError("", "Selected course or instructor does not belong to your branch");

                    // Re-populate dropdowns
                    var courses = _unitOfWork.courseRepo.GetAllCourses(true)
                        .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                        .ToList();
                    var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

                    ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", exam.CrsId);
                    ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", exam.InsId);

                    return View(exam);
                }

                try
                {
                    _unitOfWork.examRepo.UpdateExam(exam);
                    TempData["Success"] = "Exam updated successfully";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating exam: {ex.Message}");
                }
            }

            // Re-populate dropdowns if we reach here due to validation failure
            var branchCourses = _unitOfWork.courseRepo.GetAllCourses(true)
                .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                .ToList();
            var branchInstructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

            ViewBag.Courses = new SelectList(branchCourses, "CrsId", "CrsName", exam.CrsId);
            ViewBag.Instructors = new SelectList(branchInstructors, "InsId", "User.Username", exam.InsId);

            ViewData["Title"] = $"Edit Exam: {exam.ExamName}";
            return View(exam);
        }

        // GET: BranchManagerExam/Delete/5
        public IActionResult Delete(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Exam: {exam.ExamName}";
            return View(exam);
        }

        // POST: BranchManagerExam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.examRepo.DeleteExam(id);
                TempData["Success"] = "Exam deleted successfully";

                if (exam.CrsId.HasValue)
                    return RedirectToAction(nameof(Index), new { courseId = exam.CrsId });
                else
                    return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting exam: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: BranchManagerExam/GenerateRandom/5 (courseId)
        public IActionResult GenerateRandom(int courseId)
        {
            var course = _unitOfWork.courseRepo.GetCourseById(courseId);

            // Verify course exists and belongs to this branch
            if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get instructors from this branch
            var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", course.InsId);

            var exam = new Exam
            {
                CrsId = courseId,
                ExamName = $"{course.CrsName} Exam - {DateTime.Now:yyyyMMdd}",
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                InsId = course.InsId,
                Isactive = true
            };

            ViewBag.Course = course;
            ViewData["Title"] = $"Generate Random Exam for {course.CrsName}";

            return View(exam);
        }

        // POST: BranchManagerExam/GenerateRandom
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateRandom(Exam exam, int mcqCount, int tfCount)
        {
            if (ModelState.IsValid)
            {
                var course = _unitOfWork.courseRepo.GetCourseById(exam.CrsId ?? 0);
                var instructor = exam.InsId.HasValue ? _unitOfWork.instructorRepo.GetInstructorByIdWithBranch(exam.InsId.Value) : null;

                // Verify course and instructor belong to this branch
                if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId ||
                    (instructor != null && instructor.Track?.BranchId != CurrentBranchId))
                {
                    ModelState.AddModelError("", "Selected course or instructor does not belong to your branch");

                    ViewBag.Course = course;
                    var instructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);
                    ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", exam.InsId);

                    ViewData["Title"] = $"Generate Random Exam for {course?.CrsName ?? "Course"}";
                    return View(exam);
                }

                try
                {
                    // Generate random exam
                    int examId = _unitOfWork.examRepo.GenerateRandomExam(
                        exam.ExamName,
                        exam.CrsId.Value,
                        exam.InsId.Value,
                        mcqCount,
                        tfCount,
                        exam.StartTime ?? DateTime.Now.AddDays(1),
                        exam.EndTime ?? DateTime.Now.AddDays(1).AddHours(2)
                    );

                    TempData["Success"] = "Random exam generated successfully";
                    return RedirectToAction(nameof(Details), new { id = examId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error generating random exam: {ex.Message}");
                }
            }

            // If we get here, redisplay the form
            var branchCourse = _unitOfWork.courseRepo.GetCourseById(exam.CrsId ?? 0);
            var branchInstructors = _unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(CurrentBranchId, true);

            ViewBag.Course = branchCourse;
            ViewBag.Instructors = new SelectList(branchInstructors, "InsId", "User.Username", exam.InsId);

            ViewData["Title"] = $"Generate Random Exam for {branchCourse?.CrsName ?? "Course"}";
            return View(exam);
        }

        // GET: BranchManagerExam/AssignExam/5
        public IActionResult AssignExam(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get students taking this course who haven't been assigned this exam yet
            var courseStudents = _unitOfWork.studentRepo.GetStudentByCourseId(exam.CrsId ?? 0)
                .Where(s => !_unitOfWork.studentExamRepo.GetStudentExamsByExamId(id).Any(se => se.StudentId == s.StudentId))
                .ToList();

            // Create SelectListItems from the student list
            var studentItems = courseStudents.Select(s => new SelectListItem
            {
                Value = s.StudentId.ToString(),
                Text = s.User?.Username ?? $"Student {s.StudentId}"
            }).ToList();

            // Use SelectList instead of MultiSelectList to avoid the Count() issue
            ViewBag.Students = new SelectList(studentItems, "Value", "Text");

            ViewBag.Exam = exam;
            ViewData["Title"] = $"Assign Exam: {exam.ExamName}";

            return View(exam);
        }

        // POST: BranchManagerExam/AssignExam/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignExam(int id, int[] selectedStudents)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            if (selectedStudents == null || selectedStudents.Length == 0)
            {
                TempData["Error"] = "Please select at least one student";

                // Get students taking this course who haven't been assigned this exam yet
                var courseStudents = _unitOfWork.studentRepo.GetStudentByCourseId(exam.CrsId ?? 0)
                    .Where(s => !_unitOfWork.studentExamRepo.GetStudentExamsByExamId(id).Any(se => se.StudentId == s.StudentId))
                    .ToList();

                // Create SelectListItems from the student list
                var studentItems = courseStudents.Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = s.User?.Username ?? $"Student {s.StudentId}"
                }).ToList();

                // Use SelectList instead of MultiSelectList
                ViewBag.Students = new SelectList(studentItems, "Value", "Text");
                ViewBag.Exam = exam;

                ViewData["Title"] = $"Assign Exam: {exam.ExamName}";
                return View(exam);
            }

            try
            {
                int assignedCount = 0;

                // Assign exam to selected students
                foreach (var studentId in selectedStudents)
                {
                    _unitOfWork.studentRepo.AssignExamToStudent(id, studentId);
                    assignedCount++;
                }

                TempData["Success"] = $"Exam assigned successfully to {assignedCount} student(s)";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error assigning exam: {ex.Message}";
                return RedirectToAction(nameof(AssignExam), new { id });
            }
        }


        // GET: BranchManagerExam/Results/5
        public IActionResult Results(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get exam results
            var results = _unitOfWork.examRepo.GetExamResults(id);

            ViewBag.Exam = exam;
            ViewBag.PassCount = results.Count(r => r.Score >= (exam.PassedGrade ?? 0));
            ViewBag.FailCount = results.Count(r => r.Score < (exam.PassedGrade ?? 0));
            ViewBag.PassPercentage = results.Any()
                ? (double)results.Count(r => r.Score >= (exam.PassedGrade ?? 0)) / results.Count() * 100
                : 0;

            ViewData["Title"] = $"Results for {exam.ExamName}";
            return View(results);
        }

        // GET: BranchManagerExam/StudentResult/5/1 (examId/studentId)
        public IActionResult StudentResult(int examId, int studentId)
        {
            var exam = _unitOfWork.examRepo.GetExamById(examId);
            var student = _unitOfWork.studentRepo.GetStudentById(studentId);

            // Verify exam exists and belongs to this branch
            if (exam == null || exam.Ins?.Track?.BranchId != CurrentBranchId || student == null)
            {
                return NotFound();
            }

            // Get the student's exam result with answers
            var result = _unitOfWork.examRepo.GetStudentExamResult(examId, studentId);
            var answers = _unitOfWork.examRepo.GetStudentExamAnswers(examId, studentId);

            ViewBag.Exam = exam;
            ViewBag.Student = student;
            ViewBag.Result = result;
            ViewBag.IsPassed = result?.Score >= exam.PassedGrade;

            ViewData["Title"] = $"Exam Results: {student.User?.Username ?? "Student"} - {exam.ExamName}";
            return View(answers);
        }
    }
}
