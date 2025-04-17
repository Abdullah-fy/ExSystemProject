﻿using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOs.Instructor
{
    public class CreateInstructorDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'")]
        public string Gender { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Salary { get; set; }

        public int? TrackId { get; set; }
    }
}
