namespace ExSystemProject.DTOs.Student
{
    public class ShowStudentDTO
    {
        public int StudentId { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? TrackName { get; set; }
        public DateOnly? EnrollmentDate { get; set; }
        public bool? IsActive { get; set; }

    }
}
