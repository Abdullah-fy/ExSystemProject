using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class InstructorDTO
    {
        public int InsId { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value")]
        public decimal? Salary { get; set; }

        public bool? Isactive { get; set; }

        [Required(ErrorMessage = "Track is required")]
        public int? TrackId { get; set; }

        public int? UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        public string TrackName { get; set; }

        public List<CourseDTO> AssignedCourses { get; set; } = new List<CourseDTO>();

        public int? BranchId { get; set; }

        public string BranchName { get; set; }

        public string ImageUrl { get; set; }
    }
}