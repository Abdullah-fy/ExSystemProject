using System.Diagnostics.Metrics;
using ExSystemProject.Models;

namespace ExSystemProject.ViewModels
{
    public class StudentFilterViewModelForInstructor
    {
        public int? selectedCourse {  get; set; }
        public List<StudentViewModelForInstructor> students { get; set; }
        public List<Course> courses { get; set; } 
    }
}
