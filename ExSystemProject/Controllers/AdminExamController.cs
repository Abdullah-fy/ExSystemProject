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

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName");
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");

            return View();
        }

        // POST: AdminExam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExamDTO examDTO)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                var exam = _mapper.Map<Exam>(examDTO);

                // Create blank exam
                int examId = _unitOfWork.examRepo.CreateBlankExam(exam);

                return RedirectToAction(nameof(Details), new { id = examId });
            }

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

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

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", exam.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", exam.InsId);

            return View(examDTO);
        }

        // POST: AdminExam/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ExamDTO examDTO)
        {
            var userId = GetCurrentUserId();

            if (id != examDTO.ExamId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var exam = _mapper.Map<Exam>(examDTO);

                // Use the new stored procedure for updating
                _unitOfWork.examRepo.UpdateExam(exam);

                return RedirectToAction(nameof(Details), new { id = id });
            }

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

            return View(examDTO);
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

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName");
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");

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

                    TempData["SuccessMessage"] = "Random exam generated successfully!";
                    return RedirectToAction(nameof(Details), new { id = examId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

            return View(examDTO);
        }
    }
}
