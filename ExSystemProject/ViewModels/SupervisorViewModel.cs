using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ExSystemProject.ViewModels
{
    public class SupervisorViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username must be between {2} and {1} characters", MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("[MF]", ErrorMessage = "Gender must be either M or F")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        // Branch will be set automatically from the controller
        [Required(ErrorMessage = "Branch ID is required")]
        public int BranchId { get; set; }

        // Remove Required attribute from BranchName to avoid validation errors
        public string BranchName { get; set; } = ""; // Initialize with empty string

        // Track is optional for a supervisor
        public int? TrackId { get; set; }

        public bool IsActive { get; set; } = true;

        // For populating the tracks dropdown
        public List<SelectListItem> Tracks { get; set; } = new List<SelectListItem>();
    }

    public class SupervisorEditViewModel
    {
        public int AssignmentId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username must be between {2} and {1} characters", MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("[MF]", ErrorMessage = "Gender must be either M or F")]
        public string Gender { get; set; }

        // Branch will be set automatically from the controller
        public int BranchId { get; set; }
        public string BranchName { get; set; }

        // Track is optional for a supervisor
        public int? TrackId { get; set; }

        public bool IsActive { get; set; } = true;

        // For populating the tracks dropdown
        public List<SelectListItem> Tracks { get; set; } = new List<SelectListItem>();
    }
}
