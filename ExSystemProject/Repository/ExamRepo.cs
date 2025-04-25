using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExSystemProject.Repository
{
    public class ExamRepo : GenaricRepo<Exam>
    {
        private readonly ExSystemTestContext _context;

        public ExamRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        // Call stored procedure to create a blank exam
        public int CreateBlankExam(Exam exam)
        {
            // Required parameters
            var crsIdParam = new SqlParameter("@crs_id", exam.CrsId ?? throw new ArgumentNullException("Course ID is required"));
            var examNameParam = new SqlParameter("@exam_name", exam.ExamName);

            // Optional parameters
            var startTimeParam = new SqlParameter("@startTime", exam.StartTime ?? (object)DBNull.Value);
            var endTimeParam = new SqlParameter("@endTime", exam.EndTime ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", exam.InsId ?? (object)DBNull.Value);

            // Execute the stored procedure
            var result = _context.Exams
                .FromSqlRaw("EXEC sp_create_blank_exam @crs_id, @exam_name, @startTime, @endTime, @ins_id",
                    crsIdParam, examNameParam, startTimeParam, endTimeParam, insIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result?.ExamId ?? throw new Exception("Failed to create exam");
        }


        // Call stored procedure to update an exam
        public void UpdateExam(Exam exam)
        {
            var examIdParam = new SqlParameter("@exam_id", exam.ExamId);
            var examNameParam = new SqlParameter("@exam_name", exam.ExamName);
            var startTimeParam = new SqlParameter("@startTime", exam.StartTime ?? (object)DBNull.Value);
            var endTimeParam = new SqlParameter("@endTime", exam.EndTime ?? (object)DBNull.Value);
            var crsIdParam = new SqlParameter("@crs_id", exam.CrsId ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@ins_id", exam.InsId ?? (object)DBNull.Value);
            var isActiveParam = new SqlParameter("@isactive", exam.Isactive ?? (object)DBNull.Value);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateExam @exam_id, @exam_name, @startTime, @endTime, @crs_id, @ins_id, @isactive",
                examIdParam, examNameParam, startTimeParam, endTimeParam, crsIdParam, insIdParam, isActiveParam);
        }

        // Call stored procedure to delete an exam
        public void DeleteExam(int examId)
        {
            var examIdParam = new SqlParameter("@ExamID", examId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_delete_exam @ExamID",
                examIdParam);
        }

        // Call stored procedure to add question to exam
        public void AddQuestionToExam(int examId, int questionId)
        {
            var examIdParam = new SqlParameter("@ExamID", examId);
            var quesIdParam = new SqlParameter("@QuesID", questionId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_add_ques_to_exam @ExamID, @QuesID",
                examIdParam, quesIdParam);
        }

        // Call stored procedure to remove question from exam
        public void RemoveQuestionFromExam(int examId, int questionId)
        {
            var examIdParam = new SqlParameter("@ExamID", examId);
            var quesIdParam = new SqlParameter("@QuesID", questionId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_remove_ques_from_exam @ExamID, @QuesID",
                examIdParam, quesIdParam);
        }

        // Call stored procedure to get all exams
        public List<Exam> GetAllExams()
        {
            return _context.Exams
                .FromSqlRaw("EXEC sp_get_all_exams")
                .AsEnumerable()
                .ToList();
        }

        // Call stored procedure to get exam by ID
        public Exam GetExamById(int examId)
        {
            var examIdParam = new SqlParameter("@ExamID", examId);

            return _context.Exams
                .FromSqlRaw("EXEC sp_get_exam_byid @ExamID", examIdParam)
                .AsEnumerable()
                .FirstOrDefault();
        }

        // Call stored procedure to get questions by exam ID
        public List<Question> GetQuestionsByExamId(int examId)
        {
            var examIdParam = new SqlParameter("@ExamID", examId);

            return _context.Questions
                .FromSqlRaw("EXEC sp_get_questions_by_exam_id @ExamID", examIdParam)
                .AsEnumerable()
                .ToList();
        }

        // Call stored procedure to get exam questions with their choices
        public List<Question> GetExamQuestionsAndChoices(int examId)
        {
            var examIdParam = new SqlParameter("@ExamID", examId);

            // This requires special handling as it returns multiple result sets
            // You may need a custom approach or a Dapper implementation
            // This is a simplified version
            return _context.Questions
                .Include(q => q.Choices)
                .Where(q => q.ExamId == examId)
                .ToList();
        }

        public int GenerateRandomExam(string examName, int courseId, int instructorId, int mcqCount, int tfCount, DateTime startTime, DateTime endTime)
        {
            try
            {
                // First get course name as the stored procedure requires course name instead of ID
                var course = _context.Courses.Find(courseId);
                if (course == null)
                    throw new Exception($"Course with ID {courseId} not found");

                var crsNameParam = new SqlParameter("@Crs_Name", course.CrsName);
                var mcqCountParam = new SqlParameter("@MCQ_Count", mcqCount);
                var tfCountParam = new SqlParameter("@TF_Count", tfCount);
                var insIdParam = new SqlParameter("@Ins_Id", instructorId);
                var examNameParam = new SqlParameter("@ExamName", examName);
                var startTimeParam = new SqlParameter("@StartTime", startTime);
                var endTimeParam = new SqlParameter("@EndTime", endTime);

                // Execute the stored procedure without output parameter
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_GenerateRandomExam @Crs_Name, @MCQ_Count, @TF_Count, @Ins_Id, @ExamName, @StartTime, @EndTime",
                    crsNameParam, mcqCountParam, tfCountParam, insIdParam, examNameParam, startTimeParam, endTimeParam);

                // Get the last created exam as the result
                var lastExam = _context.Exams
                    .OrderByDescending(e => e.ExamId)
                    .FirstOrDefault();

                return lastExam?.ExamId ?? throw new Exception("Failed to generate exam");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating random exam: {ex.Message}", ex);
            }
        }


        public List<Exam> ExamByInstructorId(int instructorId)
        {
            return _context.Exams.Where(c => c.InsId == instructorId).ToList();
        }

        public void GenerateAndAssignExam(int SelectedCourseId, int MCQCount, int TFCount,int InsId,
                DateTime StartTime, DateTime EndTime)
        {

            _context.Database.ExecuteSqlRaw("EXEC sp_GenerateAndAssignExam @Crs_Id ,@MCQ_Count, @TF_Count , @Ins_Id ,@StartTime ,@EndTime",
               new SqlParameter("@Crs_Id", SelectedCourseId),
               new SqlParameter("@MCQ_Count", MCQCount),
               new SqlParameter("@TF_Count", TFCount),
               new SqlParameter("@Ins_Id", InsId),
               new SqlParameter("@StartTime", StartTime),
               new SqlParameter("@EndTime", EndTime));
        }

        public void GenerateAndAssignExamForStudent(int SelectedCourseId, int MCQCount, int TFCount, int InsId,
    int StudentId, DateTime StartTime, DateTime EndTime)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_GenerateAndAssignExamForStudent @Crs_Id, @MCQ_Count, @TF_Count, @Ins_Id, @StudentId, @StartTime, @EndTime",
                new SqlParameter("@Crs_Id", SelectedCourseId),
                new SqlParameter("@MCQ_Count", MCQCount),
                new SqlParameter("@TF_Count", TFCount),
                new SqlParameter("@Ins_Id", InsId),
                new SqlParameter("@StudentId", StudentId),
                new SqlParameter("@StartTime", StartTime),
                new SqlParameter("@EndTime", EndTime));
        }


     }
}
