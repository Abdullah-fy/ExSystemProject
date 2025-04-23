using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExSystemProject.Repository
{
    public class AdminQuestionRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminQuestionRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Insert MCQ question with choices
        public int InsertQuestionMCQ(Question question, List<Choice> choices)
        {
            // First, insert the question
            var questionTextParam = new SqlParameter("@QuesText", question.QuesText);
            var questionTypeParam = new SqlParameter("@QuesType", "MCQ");
            var examIdParam = new SqlParameter("@ExamId", question.ExamId ?? (object)DBNull.Value);
            var scoreParam = new SqlParameter("@QuesScore", question.QuesScore);
            var questionIdParam = new SqlParameter
            {
                ParameterName = "@QuesId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_InsertQuestion @QuesText, @QuesType, @ExamId, @QuesScore, @QuesId OUTPUT",
                questionTextParam, questionTypeParam, examIdParam, scoreParam, questionIdParam);

            int questionId = (int)questionIdParam.Value;

            // Then, insert each choice
            foreach (var choice in choices)
            {
                var choiceTextParam = new SqlParameter("@ChoiceText", choice.ChoiceText);
                // FIX: Remove the null-coalescing operator since IsCorrect is a non-nullable bool
                var isCorrectParam = new SqlParameter("@IsCorrect", choice.IsCorrect);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_InsertChoice @QuesId, @ChoiceText, @IsCorrect",
                    new SqlParameter("@QuesId", questionId), choiceTextParam, isCorrectParam);
            }

            return questionId;
        }

        // Insert True/False question
        public int InsertQuestionTF(Question question, string correctAnswer)
        {
            // First, insert the question
            var questionTextParam = new SqlParameter("@QuesText", question.QuesText);
            var questionTypeParam = new SqlParameter("@QuesType", "TF");
            var examIdParam = new SqlParameter("@ExamId", question.ExamId ?? (object)DBNull.Value);
            var scoreParam = new SqlParameter("@QuesScore", question.QuesScore);
            var questionIdParam = new SqlParameter
            {
                ParameterName = "@QuesId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_InsertQuestion @QuesText, @QuesType, @ExamId, @QuesScore, @QuesId OUTPUT",
                questionTextParam, questionTypeParam, examIdParam, scoreParam, questionIdParam);

            int questionId = (int)questionIdParam.Value;

            // Insert True choice
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_InsertChoice @QuesId, @ChoiceText, @IsCorrect",
                new SqlParameter("@QuesId", questionId),
                new SqlParameter("@ChoiceText", "True"),
                new SqlParameter("@IsCorrect", correctAnswer.ToLower() == "true"));

            // Insert False choice
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_InsertChoice @QuesId, @ChoiceText, @IsCorrect",
                new SqlParameter("@QuesId", questionId),
                new SqlParameter("@ChoiceText", "False"),
                new SqlParameter("@IsCorrect", correctAnswer.ToLower() == "false"));

            return questionId;
        }

        // Delete a question
        public void DeleteQuestion(int questionId)
        {
            var questionIdParam = new SqlParameter("@QuesId", questionId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteQuestion @QuesId",
                questionIdParam);
        }

        // Update question text
        public void UpdateQuestionText(int questionId, string questionText)
        {
            var questionIdParam = new SqlParameter("@QuesId", questionId);
            var questionTextParam = new SqlParameter("@QuesText", questionText);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpdateQuestionText @QuesId, @QuesText",
                questionIdParam, questionTextParam);
        }

        // Update question and its choices
        public void UpdateQuestionAndChoices(Question question, List<Choice> choices)
        {
            // Update the question
            UpdateQuestionText(question.QuesId, question.QuesText);

            // Delete all existing choices for this question
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_DeleteAllChoicesForQuestion @QuesId",
                new SqlParameter("@QuesId", question.QuesId));

            // Insert the new/updated choices
            foreach (var choice in choices)
            {
                var choiceTextParam = new SqlParameter("@ChoiceText", choice.ChoiceText);
                // FIX: Remove the null-coalescing operator since IsCorrect is a non-nullable bool
                var isCorrectParam = new SqlParameter("@IsCorrect", choice.IsCorrect);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_InsertChoice @QuesId, @ChoiceText, @IsCorrect",
                    new SqlParameter("@QuesId", question.QuesId), choiceTextParam, isCorrectParam);
            }
        }

        // Get all questions
        public List<Question> GetAllQuestions()
        {
            return _context.Questions
                .Include(q => q.Exam)
                .ToList();
        }

        // Get choices for a question
        public List<Choice> GetQuestionChoices(int questionId)
        {
            var questionIdParam = new SqlParameter("@QuesId", questionId);

            return _context.Choices
                .FromSqlRaw("EXEC sp_GetChoicesByQuestionId @QuesId", questionIdParam)
                .ToList();
        }
    }
}
