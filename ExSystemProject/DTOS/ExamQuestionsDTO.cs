namespace ExSystemProject.DTOS
{
    public class ExamQuestionsDTO
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string Type { get; set; }

        public List<ChoiceDTO> Choices { get; set; } = new List<ChoiceDTO>();


        public class ChoiceDTO
        {
            public int ChoiceID { get; set; }
            public int QuestionID { get; set; }
            public string ChoiceText { get; set; }
            public bool iscorrect { get; set; }
        }
    }
}
