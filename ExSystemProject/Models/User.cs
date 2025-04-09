using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public bool? Isactive { get; set; }

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public string? Img { get; set; }

    public string? Upassword { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<UserAssignment> UserAssignments { get; set; } = new List<UserAssignment>();
}
