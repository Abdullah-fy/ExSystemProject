using ExSystemProject.DTOS;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    public class CoursesController : Controller
    {
        private readonly UnitOfWork unitOfWork;

        public CoursesController(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // here wanna to get course by track id to student i will get it from cookie 

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetallcoursesbyTrackid()
        {
            // get track id from cookie 
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

            

            
            //var std = unitOfWork.studentRepo.getById(Convert.ToInt32(stdIdCookie));

            //if (std == null || std.Track == null)
            //    return NotFound();

            // catching all intructors on track 
            var instructors = unitOfWork.instructorRepo.GetInstructorsByTrackId(Convert.ToInt32(std.Track.TrackId));

            List<CourseDTO> coursesdtos = new List<CourseDTO>(); 
            // get all courses by instructors ids 

            foreach (var instructor in instructors) {
             var courses = unitOfWork.instructorRepo.GetInstructorCourses(instructor.InsId);

                foreach (var course in courses) {
                    coursesdtos.Add(new CourseDTO
                    {
                        CrsId = course.CrsId,
                        CrsName = course.CrsName,
                        CrsPeriod = course.CrsPeriod,
                        Description = course.description,
                        Poster = course.Poster, 
                        InsId = course.InsId,
                        Isactive = course.Isactive,
                        InstructorName = instructor?.User?.Username

                    });
                
                }

            }






            return View(coursesdtos); 
        }

        // view course detail + Allow Enroll on std id 

        public IActionResult GetCoursebyid(int id)
        {
            var crs = unitOfWork.courseRepo.GetCourseById(id);
            CourseDTO course = new CourseDTO()
            {
                CrsId= crs.CrsId,
                CrsName= crs.CrsName,
                CrsPeriod= crs.CrsPeriod,
                Description = crs.description,
                InsId= crs.InsId,
                Poster = crs.Poster, 
                
                
            };
            return View(course); 
           // return Content($"say hello course id =  {id}");
        }


        // enroll course to student 
        public IActionResult Enroll(int crsid)
        {
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            // checking first if student login or not 
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized(); // should redirect to login page 

            var userid = userclaim.Value;

            bool enroll = unitOfWork.studentRepo.Enrollment(Convert.ToInt32(userid),crsid);
           // int x = 0;
            if (!enroll)
            {

                TempData["ToastType"] = "warning";
                TempData["ToastMessage"] = "You are already enrolled in this course.";
            }
            else
            {
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "You have successfully enrolled in this course!";
             
            }

            return RedirectToAction("GetCoursebyid");


        }


    }
}
