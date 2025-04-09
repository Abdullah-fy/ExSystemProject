using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Question
{
    public int QuesId { get; set; }

    public string QuesText { get; set; } = null!;

    public string QuesType { get; set; } = null!;

    public int? ExamId { get; set; }

    public int QuesScore { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();

    public virtual Exam? Exam { get; set; }

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
