using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;


namespace ExSystemProject.Models
{
    public partial class Course
    {
        [Display(Name = "Course ID")]
        public int CrsId { get; set; }
        [Display(Name = "Course Name")]

        public string CrsName { get; set; } = null!;
        [Display(Name = "Num Of Days")]
        public int? CrsPeriod { get; set; }
        public int? InsId { get; set; }

        public string? description { get; set; }
        public string? Poster { get; set; }

        public bool? Isactive { get; set; }


        public string? Description { get; set; }

        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

        public virtual Instructor? Ins { get; set; }

        public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
    }
}
