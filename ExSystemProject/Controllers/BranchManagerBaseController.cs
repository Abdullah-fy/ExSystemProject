using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class BranchManagerBaseController : Controller
    {
        protected readonly UnitOfWork _unitOfWork;
        protected int CurrentBranchId { get; private set; }
        protected string CurrentBranchName { get; private set; }

        public BranchManagerBaseController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var userAssignment = _unitOfWork.userAssignmentRepo.GetUserBranchAssignment(userId);

                if (userAssignment == null || userAssignment.Isactive == false)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                    return;
                }

                CurrentBranchId = userAssignment.BranchId ?? 0;

                var branch = _unitOfWork.branchRepo.getById(CurrentBranchId);
                CurrentBranchName = branch?.BranchName ?? "Unknown";

                ViewData["BranchId"] = CurrentBranchId;
                ViewData["BranchName"] = CurrentBranchName;
                ViewData["Username"] = User.FindFirstValue(ClaimTypes.Name);
            }
        }

        protected bool IsBranchResource(int resourceBranchId)
        {
            return resourceBranchId == CurrentBranchId;
        }

        protected int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
