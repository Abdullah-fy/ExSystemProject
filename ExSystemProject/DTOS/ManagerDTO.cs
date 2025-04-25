using System;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class ManagerDTO
    {
        // UserAssignment properties
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        public int? BranchId { get; set; }

        // Not required - will be populated from BranchId
        public string? BranchName { get; set; }

        public bool? Isactive { get; set; }

        // User properties
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        // Make image optional
        public string? Img { get; set; }
    }
}
