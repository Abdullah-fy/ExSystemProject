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


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetallcoursesbyTrackid()
        {
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized(); 

            var userid = userclaim.Value;




            var std = unitOfWork.studentRepo.Getstd(Convert.ToInt32(userid));
            if (std == null || std.Track == null)
                return NotFound();

            

            
            //var std = unitOfWork.studentRepo.getById(Convert.ToInt32(stdIdCookie));

            //if (std == null || std.Track == null)
            //    return NotFound();

            // catching all intructors on track 
            var instructors = unitOfWork.instructorRepo.GetInstructorsByTrackId(Convert.ToInt32(std.Track.TrackId));

            List<CourseDTO> coursesdtos = new List<CourseDTO>(); 

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
        }


        public IActionResult Enroll(int crsid)
        {
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized(); 

            var userid = userclaim.Value;

            bool enroll = unitOfWork.studentRepo.Enrollment(Convert.ToInt32(userid),crsid);
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
