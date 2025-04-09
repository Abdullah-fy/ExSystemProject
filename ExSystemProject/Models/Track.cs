using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Track
{
    public int TrackId { get; set; }

    public string TrackName { get; set; } = null!;

    public int? TrackDuration { get; set; }

    public int? TrackIntake { get; set; }

    public bool? IsActive { get; set; }

    public int? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<UserAssignment> UserAssignments { get; set; } = new List<UserAssignment>();
}
