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
    public class ExamController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExamController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Exam
        public IActionResult Index(int? courseId = null)
        {
            List<Exam> exams;

            if (courseId.HasValue)
            {
                // Get exams for a specific course
                exams = _unitOfWork.courseRepo.GetExamsByCourseId(courseId.Value);
                ViewBag.CourseId = courseId;
                ViewBag.CourseName = _unitOfWork.courseRepo.GetCourseById(courseId.Value)?.CrsName;
            }
            else
            {
                // Get all exams
                exams = _unitOfWork.examRepo.GetAllExams();
            }

            var examDTOs = _mapper.Map<List<ExamDTO>>(exams);
            return View(examDTOs);
        }

        // GET: Exam/Details/5
        public IActionResult Details(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            // Get questions for this exam
            var questions = _unitOfWork.examRepo.GetQuestionsByExamId(id);

            var examDTO = _mapper.Map<ExamDTO>(exam);
            examDTO.Questions = _mapper.Map<List<QuestionBankDTO>>(questions);

            return View(examDTO);
        }

        // GET: Exam/Create
        public IActionResult Create()
        {
            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName");
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");

            return View();
        }

        // POST: Exam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExamDTO examDTO)
        {
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

        // GET: Exam/Edit/5
        public IActionResult Edit(int id)
        {
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

        // POST: Exam/Edit/5
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
                _unitOfWork.examRepo.UpdateExam(exam);

                

                return RedirectToAction(nameof(Details), new { id = id });
            }

            var courses = _unitOfWork.courseRepo.getAll();
            var instructors = _unitOfWork.instructorRepo.getAll();

            ViewBag.Courses = new SelectList(courses, "CrsId", "CrsName", examDTO.CrsId);
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", examDTO.InsId);

            return View(examDTO);
        }

        // GET: Exam/Delete/5
        public IActionResult Delete(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);
            if (exam == null)
                return NotFound();

            var examDTO = _mapper.Map<ExamDTO>(exam);
            return View(examDTO);
        }

        // POST: Exam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var exam = _unitOfWork.examRepo.GetExamById(id);
            int? courseId = exam?.CrsId;

            // Delete exam
            _unitOfWork.examRepo.DeleteExam(id);

            if (courseId.HasValue)
                return RedirectToAction(nameof(Index), new { courseId = courseId });
            else
                return RedirectToAction(nameof(Index));
        }

        // GET: Exam/GenerateRandomExam
        public IActionResult GenerateRandomExam()
        {
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
