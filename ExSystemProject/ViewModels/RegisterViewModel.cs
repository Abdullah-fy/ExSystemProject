using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExSystemProject.Models;

namespace ExSystemProject.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be betweeen 3 and 100 character" )]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be 6 characters at least")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }  

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

         public int? BranchId { get; set; } 
        public int? TrackId { get; set; } 

         [Range(0, 500000, ErrorMessage = "Salary must be between 0 and 500,000")]
        public decimal? Salary { get; set; } 

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }  
        public virtual IEnumerable<Branch>? Branches { get; set; }
        public virtual IEnumerable<Track>? Tracks { get; set; }
    }
}

