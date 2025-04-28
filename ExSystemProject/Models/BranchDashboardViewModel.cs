using System.Collections.Generic;

namespace ExSystemProject.Models
{
    public class BranchDashboardViewModel
    {
        public int StudentCount { get; set; }
        public int InstructorCount { get; set; }
        public int CourseCount { get; set; }
        public int TrackCount { get; set; }
        public int ExamCount { get; set; }
        public int SupervisorCount { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }

        // Chart data
        public List<string> TrackNames { get; set; } = new List<string>();
        public List<int> StudentsPerTrack { get; set; } = new List<int>();

        // Exam metrics
        public int PassedExams { get; set; }
        public int FailedExams { get; set; }
        public int NotTakenExams { get; set; }
        public int PassRate { get; set; }
        public int AverageGrade { get; set; }
        public int CourseCompletionRate { get; set; }
    }
}
