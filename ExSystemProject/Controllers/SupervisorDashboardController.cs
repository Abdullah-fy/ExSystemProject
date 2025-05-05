using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "supervisor")]
    public class SupervisorDashboardController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupervisorDashboardController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        private UserAssignment GetCurrentSupervisor()
        {
            int userId = GetCurrentUserId();
            return _unitOfWork.supervisorRepo.GetSupervisorByUserId(userId);
        }

        // GET: SupervisorDashboard
        public IActionResult Index()
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
            {
                // Log the issue for administrative review
                System.Diagnostics.Debug.WriteLine($"User ID {GetCurrentUserId()} does not have a supervisor assignment");

                // Add an error message and redirect to home page
                TempData["Error"] = "You don't have an active supervisor assignment. Please contact an administrator.";
                return RedirectToAction("Index", "Home");
            }

            var students = _unitOfWork.supervisorRepo.GetStudentsUnderSupervisor(supervisor.AssignmentId);
            var instructors = _unitOfWork.supervisorRepo.GetInstructorsUnderSupervisor(supervisor.AssignmentId);
            var courses = _unitOfWork.supervisorRepo.GetCoursesUnderSupervisor(supervisor.AssignmentId);
            var exams = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);

            ViewBag.StudentCount = students.Count;
            ViewBag.InstructorCount = instructors.Count;
            ViewBag.CourseCount = courses.Count;
            ViewBag.ExamCount = exams.Count;
            ViewBag.UpcomingExams = exams.Where(e => e.StartTime > DateTime.Now).Count();
            ViewBag.ActiveExams = exams.Where(e => e.StartTime <= DateTime.Now && e.EndTime >= DateTime.Now).Count();
            ViewBag.CompletedExams = exams.Where(e => e.EndTime < DateTime.Now).Count();

            ViewData["Title"] = "Supervisor Dashboard";

            return View(_mapper.Map<SupervisorDTO>(supervisor));
        }

        // GET: SupervisorDashboard/Students
        public IActionResult Students()
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
            {
                TempData["Error"] = "You don't have an active supervisor assignment. Please contact an administrator.";
                return RedirectToAction("Index", "Home");
            }

            var students = _unitOfWork.supervisorRepo.GetStudentsUnderSupervisor(supervisor.AssignmentId);

            ViewData["Title"] = "Students Under Supervision";

            return View(students);
        }

        // GET: SupervisorDashboard/Instructors
        public IActionResult Instructors()
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var instructors = _unitOfWork.supervisorRepo.GetInstructorsUnderSupervisor(supervisor.AssignmentId);

            ViewData["Title"] = "Instructors Under Supervision";

            return View(instructors);
        }

        // GET: SupervisorDashboard/Courses
        public IActionResult Courses()
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var courses = _unitOfWork.supervisorRepo.GetCoursesUnderSupervisor(supervisor.AssignmentId);

            ViewData["Title"] = "Courses Under Supervision";

            return View(courses);
        }

        // GET: SupervisorDashboard/Exams
        public IActionResult Exams()
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var exams = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);

            ViewData["Title"] = "Exams Under Supervision";

            return View(exams);
        }

        // GET: SupervisorDashboard/ExamDetails/5
        public IActionResult ExamDetails(int id)
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var exam = _unitOfWork.examRepo.getById(id);

            if (exam == null)
                return NotFound();

            // Ensure the exam is under this supervisor's supervision
            var examsUnderSupervisor = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);

            if (!examsUnderSupervisor.Any(e => e.ExamId == id))
                return RedirectToAction("AccessDenied", "Account");

            // Get exam questions using the examRepo instead of questionRepo
            var questions = _unitOfWork.examRepo.GetQuestionsByExamId(id);

            ViewBag.Questions = questions;
            ViewBag.QuestionCount = questions.Count;

            ViewData["Title"] = $"Exam Details: {exam.ExamName}";

            return View(exam);
        }

        // GET: SupervisorDashboard/StudentDetails/5
        public IActionResult StudentDetails(int id)
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var student = _unitOfWork.studentRepo.getById(id);

            if (student == null)
                return NotFound();

            // Ensure the student is under this supervisor's supervision
            var studentsUnderSupervisor = _unitOfWork.supervisorRepo.GetStudentsUnderSupervisor(supervisor.AssignmentId);

            if (!studentsUnderSupervisor.Any(s => s.StudentId == id))
                return RedirectToAction("AccessDenied", "Account");

            // Get student courses and exams
            var studentCourses = _unitOfWork.studentCourseRepo.GetByStudentId(id);
            var studentExams = _unitOfWork.studentExamRepo.GetByStudentId(id);

            ViewBag.StudentCourses = studentCourses;
            ViewBag.StudentExams = studentExams;

            ViewData["Title"] = $"Student Details: {student.User?.Username}";

            return View(student);
        }

        // GET: SupervisorDashboard/ApproveExam/5
        public IActionResult ApproveExam(int id)
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var exam = _unitOfWork.examRepo.getById(id);

            if (exam == null)
                return NotFound();

            // Ensure the exam is under this supervisor's supervision
            var examsUnderSupervisor = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);

            if (!examsUnderSupervisor.Any(e => e.ExamId == id))
                return RedirectToAction("AccessDenied", "Account");

            // Update exam status
            exam.Isactive = true;
            _unitOfWork.examRepo.update(exam);
            _unitOfWork.save();

            TempData["Success"] = "Exam approved successfully";
            return RedirectToAction(nameof(ExamDetails), new { id });
        }

        // GET: SupervisorDashboard/RejectExam/5
        public IActionResult RejectExam(int id)
        {
            var supervisor = GetCurrentSupervisor();

            if (supervisor == null)
                return RedirectToAction("AccessDenied", "Account");

            var exam = _unitOfWork.examRepo.getById(id);

            if (exam == null)
                return NotFound();

            // Ensure the exam is under this supervisor's supervision
            var examsUnderSupervisor = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);

            if (!examsUnderSupervisor.Any(e => e.ExamId == id))
                return RedirectToAction("AccessDenied", "Account");

            // Update exam status
            exam.Isactive = false;
            _unitOfWork.examRepo.update(exam);
            _unitOfWork.save();

            TempData["Success"] = "Exam rejected";
            return RedirectToAction(nameof(Exams));
        }
    }
}
