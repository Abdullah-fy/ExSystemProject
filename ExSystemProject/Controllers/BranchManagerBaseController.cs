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

            // Get the current user ID
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Get the branch assignment for this user
                var userAssignment = _unitOfWork.userAssignmentRepo.GetUserBranchAssignment(userId);

                if (userAssignment == null || userAssignment.Isactive == false)
                {
                    // User is not assigned to any branch or assignment is inactive
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                    return;
                }

                CurrentBranchId = userAssignment.BranchId ?? 0;

                // Get the branch name
                var branch = _unitOfWork.branchRepo.getById(CurrentBranchId);
                CurrentBranchName = branch?.BranchName ?? "Unknown";

                // Set ViewData for use in views
                ViewData["BranchId"] = CurrentBranchId;
                ViewData["BranchName"] = CurrentBranchName;
                ViewData["Username"] = User.FindFirstValue(ClaimTypes.Name);
            }
        }

        protected bool IsBranchResource(int resourceBranchId)
        {
            return resourceBranchId == CurrentBranchId;
        }

        // Helper to get current user ID
        protected int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
