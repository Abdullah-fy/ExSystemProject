using ExSystemProject.Attributes;
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

        [SqlDateTimeRange(ErrorMessage = "date can not be less than 1/1/1753 12:00:00 AM.")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        [SqlDateTimeRange(ErrorMessage = "date can not be larger than 12/31/9999 11:59:59 PM.")]
        public DateTime EndTime { get; set; }

        public List<Course> InstructorCourses { get; set; }
    }
}
