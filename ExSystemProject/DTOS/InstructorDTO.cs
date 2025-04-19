using ExSystemProject.DTOS;

public class InstructorDTO
{
    public int InsId { get; set; }
    public decimal? Salary { get; set; }
    public bool? Isactive { get; set; }
    public int? TrackId { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string? TrackName { get; set; }
    public List<CourseDTO>? AssignedCourses { get; set; }
    
    public int? BranchId { get; set; }
    public string BranchName { get; set; }

}