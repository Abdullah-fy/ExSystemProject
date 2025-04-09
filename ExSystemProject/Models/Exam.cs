using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Exam
{
    public int ExamId { get; set; }

    public string ExamName { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? CrsId { get; set; }

    public int? InsId { get; set; }

    public bool? Isactive { get; set; }

    public int? TotalMarks { get; set; }

    public int? PassedGrade { get; set; }

    public virtual Course? Crs { get; set; }

    public virtual Instructor? Ins { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
}
