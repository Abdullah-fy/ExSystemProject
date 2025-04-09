using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class StudentExam
{
    public int StudentExamId { get; set; }

    public int? ExamId { get; set; }

    public int StudentId { get; set; }

    public int Score { get; set; }

    public bool? Isactive { get; set; }

    public string? PassFail { get; set; }

    public DateOnly? ExaminationDate { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual Student Student { get; set; } = null!;
}
