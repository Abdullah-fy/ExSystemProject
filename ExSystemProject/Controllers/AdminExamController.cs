using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace ExSystemProject.Controllers
{
    public class AdminExamController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminExamController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: AdminExam
        public IActionResult Index(int? courseId = null)
        {
            List<Exam> exams;

            if (courseId.HasValue)
            {
                // Get exams for a specific course
                exams = _unitOfWork.adminCourseRepo.GetExamsByCourseId(courseId.Value);
                ViewBag.CourseId = courseId;
                ViewBag.CourseName = _unitOfWork.adminCourseRepo.GetCourseById(courseId.Value)?.CrsName;
            }
            else
            {
                // Get all exams
                exams = _unitOfWork.adminExamRepo.GetAllExams();
            }

            var examDTOs = _mapper.Map<List<ExamDTO>>(exams);
            return View(examDTOs);
        }

        // GET: AdminExam/Details/5
        public IActionResult Details(int id)
        {
            var exam = _unitOfWork.adminExamRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            // Get questions for this exam
            var questions = _unitOfWork.adminExamRepo.GetQuestionsByExamId(id);

            var examDTO = _mapper.Map<ExamDTO>(exam);
            examDTO.Questions = _mapper.Map<List<QuestionBankDTO>>(questions);

            return View(examDTO);
        }

        // GET: AdminExam/Create
        public IActionResult Create()
        {
            var courses = _unitOfWork.adminCourseRepo.GetAllCourses(true); // Get only active courses
            var instructors = _unitOfWork.adminInstructorRepo.GetAllInstructorsWithBranch(true); // Get only active instructors

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName");
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");

            return View();
        }

        // POST: AdminExam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExamDTO examDTO)
        {
            if (ModelState.IsValid)
            {
                var exam = _mapper.Map<Exam>(examDTO);

                // Create blank exam
                int examId = _unitOfWork.adminExamRepo.CreateBlankExam(exam);

                return RedirectToAction(nameof(Details), new { id = examId });
            }

            var courses = _unitOfWork.adminCourseRepo.GetAllCourses(true);
            var instructors = _unitOfWork.adminInstructorRepo.GetAllInstructorsWithBranch(true);

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

            return View(examDTO);
        }

        // GET: AdminExam/Edit/5
        public IActionResult Edit(int id)
        {
            var exam = _unitOfWork.adminExamRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            var examDTO = _mapper.Map<ExamDTO>(exam);

            var courses = _unitOfWork.adminCourseRepo.GetAllCourses(true);
            var instructors = _unitOfWork.adminInstructorRepo.GetAllInstructorsWithBranch(true);

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", exam.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", exam.InsId);

            return View(examDTO);
        }

        // POST: AdminExam/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ExamDTO examDTO)
        {
            if (id != examDTO.ExamId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var exam = _mapper.Map<Exam>(examDTO);

                // Use the new stored procedure for updating
                _unitOfWork.adminExamRepo.UpdateExam(exam);

                return RedirectToAction(nameof(Details), new { id = id });
            }

            var courses = _unitOfWork.adminCourseRepo.GetAllCourses(true);
            var instructors = _unitOfWork.adminInstructorRepo.GetAllInstructorsWithBranch(true);

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

            return View(examDTO);
        }

        // GET: AdminExam/Delete/5
        public IActionResult Delete(int id)
        {
            var exam = _unitOfWork.adminExamRepo.GetExamById(id);
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
            var exam = _unitOfWork.adminExamRepo.GetExamById(id);
            int? courseId = exam?.CrsId;

            // Delete exam
            _unitOfWork.adminExamRepo.DeleteExam(id);

            if (courseId.HasValue)
                return RedirectToAction(nameof(Index), new { courseId = courseId });
            else
                return RedirectToAction(nameof(Index));
        }

        // GET: AdminExam/GenerateRandomExam
        public IActionResult GenerateRandomExam()
        {
            var courses = _unitOfWork.adminCourseRepo.GetAllCourses(true);
            var instructors = _unitOfWork.adminInstructorRepo.GetAllInstructorsWithBranch(true);

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
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate random exam using the counts from the form
                    int examId = _unitOfWork.adminExamRepo.GenerateRandomExam(
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

            var courses = _unitOfWork.adminCourseRepo.GetAllCourses(true);
            var instructors = _unitOfWork.adminInstructorRepo.GetAllInstructorsWithBranch(true);

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

            return View(examDTO);
        }
    }
}
