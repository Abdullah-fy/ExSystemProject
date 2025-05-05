namespace ExSystemProject.DTOS
{
    public class GetAssignExamToStudentDTO
    {
        public int ExamID { get; set; }
        public string ExamName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateOnly ExamDate { get; set; }
    }
}
