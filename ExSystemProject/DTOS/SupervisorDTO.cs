using System;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class SupervisorDTO
    {
        public int AssignmentId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        public int? BranchId { get; set; }

        public string BranchName { get; set; }

        public int? TrackId { get; set; }

        public string TrackName { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be between {2} and {1} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string? ImageUrl { get; set; }
    }
}
