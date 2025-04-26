using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

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

            // Get counts for the dashboard - remove async/await
            var studentCount = _unitOfWork.studentRepo.GetStudentCountByBranchAsync(CurrentBranchId);
            var instructorCount = _unitOfWork.instructorRepo.GetInstructorCountByBranchAsync(CurrentBranchId);
            var courseCount = _unitOfWork.courseRepo.GetCourseCountByBranchAsync(CurrentBranchId);
            var trackCount = _unitOfWork.trackRepo.GetTrackCountByBranchAsync(CurrentBranchId);
            var examCount = _unitOfWork.examRepo.GetExamCountByBranchAsync(CurrentBranchId);

            // Create a dashboard view model
            var dashboardViewModel = new BranchDashboardViewModel
            {
                StudentCount = studentCount,
                InstructorCount = instructorCount,
                CourseCount = courseCount,
                TrackCount = trackCount,
                ExamCount = examCount,
                BranchId = CurrentBranchId,
                BranchName = CurrentBranchName
            };

            return View(dashboardViewModel);
        }
    }
}
