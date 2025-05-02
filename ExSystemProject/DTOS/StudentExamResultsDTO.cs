namespace ExSystemProject.DTOS
{
    public class StudentExamResultsDTO
    {
        public string ExamName { get; set; }
        public DateTime StartTime { get; set; }
        public int TotalMarks { get; set; }
        public int Score { get; set; }
        public float? Percentage { get; set; }
        public string? Result { get; set; }
    }
}
