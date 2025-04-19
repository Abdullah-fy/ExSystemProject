using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class StudentDTO
    {
        public int StudentId { get; set; }

        [Display(Name = "Track")]
        public int? TrackId { get; set; }
        public string TrackName { get; set; }

        [Display(Name = "Branch")]
        public int? BranchId { get; set; }
        public string BranchName { get; set; }

        public DateTime? EnrollmentDate { get; set; }
        public int? UserId { get; set; }
        public bool? Isactive { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        // Navigation properties 
        public List<StudentCourseDTO> StudentCourses { get; set; } = new List<StudentCourseDTO>();
        public List<StudentExamDTO> StudentExams { get; set; } = new List<StudentExamDTO>();
    }
}
