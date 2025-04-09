using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class StudentCourse
{
    public int CrsId { get; set; }

    public int StudentId { get; set; }

    public int? Grade { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? EnrolledAt { get; set; }

    public bool? Ispassed { get; set; }

    public virtual Course Crs { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
