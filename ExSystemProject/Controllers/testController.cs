using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.Controllers
{
   // [Authorize(Roles = "instructor")]
    public class testController : Controller
    {
        public UnitOfWork unit { get; }
        public testController(UnitOfWork unit)
        {
            this.unit = unit;
        }
        public IActionResult Index()
        {
            var x = unit.branchRepo.getAll();
            return View(x);
        }


        public IActionResult Details(int id) { 
        var std = unit.studentRepo.GetStudentById(id); 
       // var std2 = unit.studentRepo.getById(id); 


        return Content($"detail : {std.StudentId} , {std.UserId}");
           
        }

        public IActionResult courses()
        {
            var c = unit.instructorRepo.GetInstructorCourses(3);
            foreach (var c2 in c)
            {
                return Content($"name = {c2.CrsName} , {c2.description} , {c2.Poster}");
            }
            return View(c);

        }

    }
}
