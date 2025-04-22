using ExSystemProject.Models;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.ViewModels
{
    public class ExamCreationViewModel
    {
        public int SelectedCourseId { get; set; }

        [Display(Name = "MCQ Questions Count")]
        public int MCQCount { get; set; }

        [Display(Name = "True/False Questions Count")]
        public int TFCount { get; set; }

        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        public List<Course> InstructorCourses { get; set; }
    }
}
