namespace ExSystemProject.DTOS
{
    public class StudentDTO
    {
        public int StudentId { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public bool? Isactive { get; set; }
        public int? TrackId { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? TrackName { get; set; }
    }
}
