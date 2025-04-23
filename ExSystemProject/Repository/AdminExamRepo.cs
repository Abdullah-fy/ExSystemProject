using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExSystemProject.Repository
{
    public class AdminExamRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminExamRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Create a blank exam
        public int CreateBlankExam(Exam exam)
        {
            var nameParam = new SqlParameter("@ExamName", exam.ExamName);
            var startTimeParam = new SqlParameter("@StartTime", exam.StartTime ?? (object)DBNull.Value);
            var endTimeParam = new SqlParameter("@EndTime", exam.EndTime ?? (object)DBNull.Value);
            var crsIdParam = new SqlParameter("@CrsId", exam.CrsId ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@InsId", exam.InsId ?? (object)DBNull.Value);
            var totalMarksParam = new SqlParameter("@TotalMarks", exam.TotalMarks ?? (object)DBNull.Value);
            var passedGradeParam = new SqlParameter("@PassedGrade", exam.PassedGrade ?? (object)DBNull.Value);
            var examIdParam = new SqlParameter
            {
                ParameterName = "@ExamId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_CreateExam @ExamName, @StartTime, @EndTime, @CrsId, @InsId, @TotalMarks, @PassedGrade, @ExamId OUTPUT",
                nameParam, startTimeParam, endTimeParam, crsIdParam, insIdParam, totalMarksParam, passedGradeParam, examIdParam);

            return (int)examIdParam.Value;
        }

        // Update an exam
        public void UpdateExam(Exam exam)
        {
            var examIdParam = new SqlParameter("@ExamId", exam.ExamId);
            var nameParam = new SqlParameter("@ExamName", exam.ExamName);
            var startTimeParam = new SqlParameter("@StartTime", exam.StartTime ?? (object)DBNull.Value);
            var endTimeParam = new SqlParameter("@EndTime", exam.EndTime ?? (object)DBNull.Value);
            var crsIdParam = new SqlParameter("@CrsId", exam.CrsId ?? (object)DBNull.Value);
            var insIdParam = new SqlParameter("@InsId", exam.InsId ?? (object)DBNull.Value);
            var isActiveParam = new SqlParameter("@IsActive", exam.Isactive ?? true);
            var totalMarksParam = new SqlParameter("@TotalMarks", exam.TotalMarks ?? (object)DBNull.Value);
            var passedGradeParam = new SqlParameter("@PassedGrade", exam.PassedGrade ?? (object)DBNull.Value);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateExam @ExamId, @ExamName, @StartTime, @EndTime, @CrsId, @InsId, @IsActive, @TotalMarks, @PassedGrade",
                examIdParam, nameParam, startTimeParam, endTimeParam, crsIdParam, insIdParam, isActiveParam, totalMarksParam, passedGradeParam);
        }

        // Delete an exam
        public void DeleteExam(int examId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteExam @ExamId", examIdParam);
        }

        // Add a question to an exam
        public void AddQuestionToExam(int examId, int questionId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);
            var questionIdParam = new SqlParameter("@QuesId", questionId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_AddQuestionToExam @ExamId, @QuesId",
                examIdParam, questionIdParam);
        }

        // Remove a question from an exam
        public void RemoveQuestionFromExam(int examId, int questionId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);
            var questionIdParam = new SqlParameter("@QuesId", questionId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_RemoveQuestionFromExam @ExamId, @QuesId",
                examIdParam, questionIdParam);
        }

        // Get all exams
        public List<Exam> GetAllExams()
        {
            return _context.Exams
                .Include(e => e.Crs)
                .Include(e => e.Ins)
                    .ThenInclude(i => i.User)
                .ToList();
        }

        // Get exam by ID
        public Exam GetExamById(int examId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);

            return _context.Exams
                .FromSqlRaw("EXEC sp_GetExamById @ExamId", examIdParam)
                .Include(e => e.Crs)
                .Include(e => e.Ins)
                    .ThenInclude(i => i.User)
                .AsEnumerable()
                .FirstOrDefault();
        }

        // Get questions by exam ID
        public List<Question> GetQuestionsByExamId(int examId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);

            return _context.Questions
                .FromSqlRaw("EXEC sp_GetQuestionsByExamId @ExamId", examIdParam)
                .ToList();
        }

        // Get exam questions and choices
        public List<Question> GetExamQuestionsAndChoices(int examId)
        {
            var examIdParam = new SqlParameter("@ExamId", examId);

            var questions = _context.Questions
                .FromSqlRaw("EXEC sp_GetQuestionsByExamId @ExamId", examIdParam)
                .Include(q => q.Choices)
                .ToList();

            return questions;
        }

        // Generate a random exam
        public int GenerateRandomExam(string examName, int courseId, int instructorId, int mcqCount, int tfCount, DateTime startTime, DateTime endTime)
        {
            var examNameParam = new SqlParameter("@ExamName", examName);
            var courseIdParam = new SqlParameter("@CourseId", courseId);
            var instructorIdParam = new SqlParameter("@InstructorId", instructorId);
            var mcqCountParam = new SqlParameter("@MCQCount", mcqCount);
            var tfCountParam = new SqlParameter("@TFCount", tfCount);
            var startTimeParam = new SqlParameter("@StartTime", startTime);
            var endTimeParam = new SqlParameter("@EndTime", endTime);
            var examIdParam = new SqlParameter
            {
                ParameterName = "@ExamId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_GenerateRandomExam @ExamName, @CourseId, @InstructorId, @MCQCount, @TFCount, @StartTime, @EndTime, @ExamId OUTPUT",
                examNameParam, courseIdParam, instructorIdParam, mcqCountParam, tfCountParam, startTimeParam, endTimeParam, examIdParam);

            return (int)examIdParam.Value;
        }
    }
}
