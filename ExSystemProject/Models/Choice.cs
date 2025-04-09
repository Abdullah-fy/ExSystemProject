using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Choice
{
    public int ChoiceId { get; set; }

    public string ChoiceText { get; set; } = null!;

    public int QuesId { get; set; }

    public bool IsCorrect { get; set; }

    public virtual Question Ques { get; set; } = null!;

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
