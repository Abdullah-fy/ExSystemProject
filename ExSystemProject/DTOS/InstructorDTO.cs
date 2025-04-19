
using System;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class InstructorDTO
    {
        public int InsId { get; set; }
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        [Display(Name = "Salary")]
        [DataType(DataType.Currency)]
        public decimal? Salary { get; set; }

        [Required(ErrorMessage = "Track is required")]
        [Display(Name = "Track")]
        public int? TrackId { get; set; }

        public string TrackName { get; set; }

        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        public string BranchName { get; set; }

        public bool? Isactive { get; set; }

        public string ImageUrl { get; set; }
    }
}
