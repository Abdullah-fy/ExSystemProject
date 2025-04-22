namespace ExSystemProject.DTOS
{
    public class StudentExamDTO
    {
        public int StudentExamId { get; set; }
        public int? ExamId { get; set; }
        public int StudentId { get; set; }
        public int Score { get; set; }
        public bool? Isactive { get; set; }
        public string PassFail { get; set; }
        public DateTime? ExaminationDate { get; set; }
        // Navigation properties
        public string ExamName { get; set; }
        public string CourseName { get; set; }
    }
}
