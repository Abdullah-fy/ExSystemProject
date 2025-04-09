using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class UserAssignment
{
    public int AssignmentId { get; set; }

    public int UserId { get; set; }

    public int? BranchId { get; set; }

    public bool? Isactive { get; set; }

    public int? TrackId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Track? Track { get; set; }

    public virtual User User { get; set; } = null!;
}
