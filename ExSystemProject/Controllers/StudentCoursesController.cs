using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    public class StudentCoursesController : Controller
    {
        private readonly UnitOfWork unitOfWork;

        public StudentCoursesController(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Getstudentcourses()
        {
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            // checking first if student login or not 
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized(); // should redirect to login page 

            var userid = userclaim.Value;

            //var trackid = Request.Cookies["TrackId"];

            //if (string.IsNullOrEmpty(trackid))
            //    return Unauthorized(); // or redirect to login


            var std = unitOfWork.studentRepo.Getstd(Convert.ToInt32(userid));
            if (std == null || std.Track == null)
                return NotFound();

            var courses = await unitOfWork.studentCourseRepo.GetStudentCoursesAsync(std.StudentId);


            return View(courses);
        }
    }
}
