using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Course
{
    public int CrsId { get; set; }

    public string CrsName { get; set; } = null!;

    public int? CrsPeriod { get; set; }

    public int? InsId { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual Instructor? Ins { get; set; }

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
