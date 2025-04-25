using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminController : SuperAdminBaseController
    {
        public AdminController(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public IActionResult Index()
        {
            var userId = GetCurrentUserId();

            // Get count statistics
            ViewBag.BranchCount = _unitOfWork.branchRepo.getAll().Count;
            ViewBag.ActiveBranchCount = _unitOfWork.branchRepo.GetAllActive().Count;

            var tracks = _unitOfWork.trackRepo.getAll();
            ViewBag.TrackCount = tracks.Count;
            ViewBag.ActiveTrackCount = tracks.Where(t => t.IsActive == true).Count();

            var courses = _unitOfWork.courseRepo.GetAllCourses(null);
            ViewBag.CourseCount = courses.Count;
            ViewBag.ActiveCourseCount = courses.Where(c => c.Isactive == true).Count();

            var students = _unitOfWork.studentRepo.GetAllStudents(null);
            ViewBag.StudentCount = students.Count;
            ViewBag.ActiveStudentCount = students.Where(s => s.Isactive == true).Count();

            var instructors = _unitOfWork.instructorRepo.GetAllInstructorsWithBranch(null);
            ViewBag.InstructorCount = instructors.Count;
            ViewBag.ActiveInstructorCount = instructors.Where(i => i.Isactive == true).Count();

            var exams = _unitOfWork.examRepo.getAll();
            ViewBag.ExamCount = exams.Count;
            ViewBag.ActiveExamCount = exams.Where(e => e.Isactive == true).Count();

            // Get branch distribution data for charts
            var branches = _unitOfWork.branchRepo.getAll();
            var branchNames = branches.Select(b => b.BranchName).ToList();
            var branchIds = branches.Select(b => b.BranchId).ToList();

            var studentsPerBranch = new List<int>();
            var instructorsPerBranch = new List<int>();
            var tracksPerBranch = new List<int>();

            foreach (var branchId in branchIds)
            {
                studentsPerBranch.Add(_unitOfWork.studentRepo.GetStudentsByBranchId(branchId).Count);
                instructorsPerBranch.Add(_unitOfWork.instructorRepo.GetInstructorsByBranchWithBranch(branchId).Count);
                tracksPerBranch.Add(_unitOfWork.trackRepo.GetTracksByBranchId(branchId).Count);
            }

            ViewBag.BranchNames = branchNames;
            ViewBag.StudentsPerBranch = studentsPerBranch;
            ViewBag.InstructorsPerBranch = instructorsPerBranch;
            ViewBag.TracksPerBranch = tracksPerBranch;

            return View();
        }
    }
}
