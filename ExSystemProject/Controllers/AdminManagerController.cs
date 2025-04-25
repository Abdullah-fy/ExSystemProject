using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.Controllers
{
    public class AdminManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
