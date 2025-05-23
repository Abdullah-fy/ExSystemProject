﻿using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.DTOS
{
    public class CourseDTO
    {
        public int CrsId { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        public string CrsName { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 hour")]
        public int? CrsPeriod { get; set; }

        public int? InsId { get; set; }
        public bool? Isactive { get; set; }
        public string? InstructorName { get; set; }
        public string? Description { get; set; }
        public string? Poster { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        public int? BranchId { get; set; }

        [Required(ErrorMessage = "Track is required")]
        public int? TrackId { get; set; }

        public string? BranchName { get; set; }
        public string? TrackName { get; set; }
    }
}
