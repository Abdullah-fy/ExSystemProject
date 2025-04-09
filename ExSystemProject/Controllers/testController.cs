using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.Controllers
{
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
    }
}
