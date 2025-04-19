namespace ExSystemProject.DTOS
{
    public class StudentCourseDTO
    {
        public int CrsId { get; set; }
        public int StudentId { get; set; }
        public int? Grade { get; set; }
        public bool? Isactive { get; set; }
        public DateTime? EnrolledAt { get; set; }
        public bool? Ispassed { get; set; }
        // Navigation properties
        public string CourseName { get; set; }
        public int? CoursePeriod { get; set; }
    }
}
