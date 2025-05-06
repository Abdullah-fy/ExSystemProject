namespace ExSystemProject.DTOS
{
    public class CourseNewDTO
    {
        public int CrsId { get; set; }
        public string CrsName { get; set; }
        public int? CrsPeriod { get; set; }
        public int? InsId { get; set; }
        public string? Description { get; set; }
        public bool? Isactive { get; set; }
        public string? InstructorName { get; set; }
        public string? TrackName { get; set; }
        public string? BranchName { get; set; }
        public int? TrackId { get; set; }
        public int? BranchId { get; set; }
    }
}
