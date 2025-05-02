namespace ExSystemProject.DTOS
{
    public class QuestionDTO
    {
        public int QuesId { get; set; }
        public string QuesText { get; set; }
        public string QuesType { get; set; }
        public int QuesScore { get; set; }
        public List<ChoiceDTO> Choices { get; set; } = new List<ChoiceDTO>();
    }
}
