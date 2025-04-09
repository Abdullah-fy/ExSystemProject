using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class StudentAnswer
{
    public int Answerid { get; set; }

    public int Studentid { get; set; }

    public int? QuesId { get; set; }

    public int ChoiceId { get; set; }

    public virtual Choice Choice { get; set; } = null!;

    public virtual Question? Ques { get; set; }

    public virtual Student Student { get; set; } = null!;
}
