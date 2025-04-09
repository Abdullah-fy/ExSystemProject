using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Instructor
{
    public int InsId { get; set; }

    public decimal? Salary { get; set; }

    public int? UserId { get; set; }

    public bool? Isactive { get; set; }

    public int? TrackId { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual Track? Track { get; set; }

    public virtual User? User { get; set; }
}
