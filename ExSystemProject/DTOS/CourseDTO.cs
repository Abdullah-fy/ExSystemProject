namespace ExSystemProject.DTOS
{
    public class CourseDTO
    {
        public int CrsId { get; set; }
        public string CrsName { get; set; }
        public int? CrsPeriod { get; set; }
        public string Description { get; set; }
        public string Poster { get; set; }
        public int? InsId { get; set; }
        public bool? Isactive { get; set; }
        public string? InstructorName { get; set; }
    }
}
