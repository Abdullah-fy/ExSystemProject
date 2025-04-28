using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ExSystemProject.Controllers
{
    public class BranchManagerController : BranchManagerBaseController
    {
        public BranchManagerController(UnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Branch Dashboard";

            // Basic counts for the dashboard
            var studentCount = _unitOfWork.studentRepo.GetStudentCountByBranchAsync(CurrentBranchId);
            var instructorCount = _unitOfWork.instructorRepo.GetInstructorCountByBranchAsync(CurrentBranchId);
            var courseCount = _unitOfWork.courseRepo.GetCourseCountByBranchAsync(CurrentBranchId);
            var trackCount = _unitOfWork.trackRepo.GetTrackCountByBranchAsync(CurrentBranchId);
            var examCount = _unitOfWork.examRepo.GetExamCountByBranchAsync(CurrentBranchId);
            var supervisorCount = _unitOfWork.supervisorRepo.GetSupervisorCountByBranchAsync(CurrentBranchId);

            // Get track data for the chart
            var tracks = _unitOfWork.trackRepo.GetActiveTracksByBranchId(CurrentBranchId);
            var trackNames = tracks.Select(t => t.TrackName).ToList();
            var studentsPerTrack = tracks.Select(t =>
                _unitOfWork.studentRepo.GetStudentsByTrackId(t.TrackId, true).Count()).ToList();

            // Get exam performance metrics
            var branchExams = _unitOfWork.examRepo.GetAllExams()
                .Where(e => e.Ins?.Track?.BranchId == CurrentBranchId)
                .ToList();

            int totalExamsTaken = 0;
            int passedExams = 0;
            int failedExams = 0;
            int notTakenExams = 0;
            double totalScores = 0;

            foreach (var exam in branchExams)
            {
                var results = _unitOfWork.studentExamRepo.GetStudentExamsByExamId(exam.ExamId);
                totalExamsTaken += results.Count;
                passedExams += results.Count(r => r.Score >= exam.PassedGrade);
                failedExams += results.Count(r => r.Score < exam.PassedGrade);
                notTakenExams += studentCount - results.Count;
                totalScores += results.Sum(r => r.Score ?? 0); // Add null check
            }

            // Calculate metrics
            double passRate = totalExamsTaken > 0 ? (double)passedExams / totalExamsTaken * 100 : 0;
            double averageGrade = totalExamsTaken > 0 ? totalScores / totalExamsTaken : 0;

            // Calculate course completion rate
            var studentCourses = _unitOfWork.studentCourseRepo.GetAllStudentCoursesByBranch(CurrentBranchId);
            var totalEnrollments = studentCourses.Count;
            var completedCourses = studentCourses.Count(sc => sc.Ispassed == true);
            double courseCompletionRate = totalEnrollments > 0 ? (double)completedCourses / totalEnrollments * 100 : 0;

            // Create a dashboard view model
            var dashboardViewModel = new BranchDashboardViewModel
            {
                StudentCount = studentCount,
                InstructorCount = instructorCount,
                CourseCount = courseCount,
                TrackCount = trackCount,
                ExamCount = examCount,
                SupervisorCount = supervisorCount,
                BranchId = CurrentBranchId,
                BranchName = CurrentBranchName,

                // Chart data
                TrackNames = trackNames,
                StudentsPerTrack = studentsPerTrack,

                // Exam metrics
                PassedExams = passedExams,
                FailedExams = failedExams,
                NotTakenExams = notTakenExams,
                PassRate = (int)passRate,
                AverageGrade = (int)averageGrade,
                CourseCompletionRate = (int)courseCompletionRate
            };

            return View(dashboardViewModel);
        }
    }
}
