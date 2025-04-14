namespace ExSystemProject.DTOS
{
    public class QuestionBankDTO
    {
        public int QuesId { get; set; }
        public string QuesText { get; set; }
        public string QuesType { get; set; }
        public int? ExamId { get; set; }
        public string? ExamName { get; set; }
        public int QuesScore { get; set; }
        public bool? Isactive { get; set; }
        public List<ChoiceDTO>? Choices { get; set; }
    }

    public class ChoiceDTO
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
