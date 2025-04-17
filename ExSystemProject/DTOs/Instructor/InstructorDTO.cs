namespace ExSystemProject.DTOs.Instructor
{
    public class InstructorDTO
    {
        public int InsId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public decimal? Salary { get; set; }
        public string TrackName { get; set; }
        public bool IsActive { get; set; }
    }
}
