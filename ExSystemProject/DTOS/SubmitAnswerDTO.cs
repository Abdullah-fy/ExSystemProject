namespace ExSystemProject.DTOS
{
    public class SubmitAnswerDTO
    {
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }
    }
}
