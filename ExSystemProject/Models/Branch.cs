using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Branch
{
    public int BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public string Location { get; set; } = null!;

    public bool? Isactive { get; set; }

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

    public virtual ICollection<UserAssignment> UserAssignments { get; set; } = new List<UserAssignment>();
}
