namespace ExSystemProject.DTOS
{
    public class ExamDTO
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? CrsId { get; set; }
        public string? CourseName { get; set; }
        public int? InsId { get; set; }
        public string? InstructorName { get; set; }
        public bool? Isactive { get; set; }
        public int? TotalMarks { get; set; }
        public int? PassedGrade { get; set; }
        public List<QuestionBankDTO>? Questions { get; set; }
    }
}
