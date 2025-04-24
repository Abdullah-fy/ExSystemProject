using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.Models;

public partial class StudentCourse
{
    [Display(Name = "Course Name")]
    public int CrsId { get; set; }
    [Display(Name ="Student Name")]
    public int StudentId { get; set; }

    public int? Grade { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? EnrolledAt { get; set; }

    public bool? Ispassed { get; set; }

    public virtual Course Crs { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
