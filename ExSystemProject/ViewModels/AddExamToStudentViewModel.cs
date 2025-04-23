namespace ExSystemProject.ViewModels
{
    public class AddExamToStudentViewModel
    {
        public int SelectedCourseId { get; set; }
        public int InstructorId { get; set; }
        public int StudentId { get; set; }
        public int MCQCount { get; set; }
        public int TFCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
