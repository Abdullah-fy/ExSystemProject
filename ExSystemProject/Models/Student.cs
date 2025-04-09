using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int? TrackId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }

    public int? UserId { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();

    public virtual Track? Track { get; set; }

    public virtual User? User { get; set; }
}
