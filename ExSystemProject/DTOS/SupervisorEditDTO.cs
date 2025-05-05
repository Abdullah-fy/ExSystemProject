using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class SupervisorEditDTO
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

        public string? ImageUrl { get; set; }
    }
}
