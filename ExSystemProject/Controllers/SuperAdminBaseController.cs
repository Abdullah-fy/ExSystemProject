using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExSystemProject.UnitOfWorks;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public abstract class SuperAdminBaseController : Controller
    {
        protected readonly UnitOfWork _unitOfWork;

        public SuperAdminBaseController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
