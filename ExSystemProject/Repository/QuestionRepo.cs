using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ExSystemProject.Repository
{
    public class QuestionRepo : GenaricRepo<Question>
    {
        private readonly ExSystemTestContext _context;

        public QuestionRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        public int InsertQuestionMCQ(Question question, List<Choice> choices)
        {
            if (choices.Count < 4)
                throw new ArgumentException("MCQ questions require at least 4 choices");

            int correctChoiceNo = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].IsCorrect)
                {
                    correctChoiceNo = i + 1;
                    break;
                }
            }

            if (correctChoiceNo == 0)
                throw new ArgumentException("One choice must be marked as correct");

            // Always set questions to active when creating
            question.Isactive = true;

            var quesTextParam = new SqlParameter("@ques_text", question.QuesText);
            var choice1Param = new SqlParameter("@choice1", choices[0].ChoiceText);
            var choice2Param = new SqlParameter("@choice2", choices[1].ChoiceText);
            var choice3Param = new SqlParameter("@choice3", choices[2].ChoiceText);
            var choice4Param = new SqlParameter("@choice4", choices[3].ChoiceText);
            var quesScoreParam = new SqlParameter("@ques_score", question.QuesScore);
            var correctChoiceNoParam = new SqlParameter("@correct_choice_no", correctChoiceNo);
            var examIdParam = new SqlParameter("@exam_id", question.ExamId ?? (object)DBNull.Value);

            // Execute stored procedure
            var result = _context.Questions
                .FromSqlRaw("EXEC sp_insert_ques_mcq @ques_text, @choice1, @choice2, @choice3, @choice4, @ques_score, @correct_choice_no, @exam_id",
                    quesTextParam, choice1Param, choice2Param, choice3Param, choice4Param, quesScoreParam, correctChoiceNoParam, examIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result?.QuesId ?? 0;
        }

        public int InsertQuestionTF(Question question, bool isTrue)
        {
            // Always set questions to active when creating
            question.Isactive = true;

            var quesTextParam = new SqlParameter("@ques_text", question.QuesText);
            var quesScoreParam = new SqlParameter("@ques_score", question.QuesScore);
            var correctAnswerParam = new SqlParameter("@correct_answer", isTrue ? 1 : 0);
            var examIdParam = new SqlParameter("@exam_id", question.ExamId ?? (object)DBNull.Value);

            // Execute stored procedure
            var result = _context.Questions
                .FromSqlRaw("EXEC sp_insert_ques_tf @ques_text, @ques_score, @correct_answer, @exam_id",
                    quesTextParam, quesScoreParam, correctAnswerParam, examIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result?.QuesId ?? 0;
        }

        // Call stored procedure to delete a question
        public void DeleteQuestion(int questionId)
        {
            try
            {
                var quesIdParam = new SqlParameter("@QuesID", questionId);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_delete_question @QuesID",
                    quesIdParam);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot delete question with ID {questionId}. It may have student answers associated with it.", ex);
            }
        }

        // Call stored procedure to update question text
        public void UpdateQuestionText(int questionId, string questionText)
        {
            var quesIdParam = new SqlParameter("@ques_id", questionId);
            var quesTextParam = new SqlParameter("@ques_text", questionText);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_update_question_text @ques_id, @ques_text",
                quesIdParam, quesTextParam);
        }

        public void UpdateQuestionAndChoices(Question question, List<Choice> choices)
        {
            if (question.QuesType == "MCQ" && choices.Count >= 4)
            {
                int correctChoiceNo = 0;
                for (int i = 0; i < choices.Count; i++)
                {
                    if (choices[i].IsCorrect)
                    {
                        correctChoiceNo = i + 1;
                        break;
                    }
                }

                var quesIdParam = new SqlParameter("@ques_id", question.QuesId);
                var quesTextParam = new SqlParameter("@ques_text", question.QuesText);
                var choice1Param = new SqlParameter("@choice1", choices[0].ChoiceText);
                var choice2Param = new SqlParameter("@choice2", choices[1].ChoiceText);
                var choice3Param = new SqlParameter("@choice3", choices[2].ChoiceText);
                var choice4Param = new SqlParameter("@choice4", choices[3].ChoiceText);
                var quesScoreParam = new SqlParameter("@ques_score", question.QuesScore);
                var correctChoiceNoParam = new SqlParameter("@correct_choice_no", correctChoiceNo);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_update_question_and_choices @ques_id, @ques_text, @choice1, @choice2, @choice3, @choice4, @ques_score, @correct_choice_no",
                    quesIdParam, quesTextParam, choice1Param, choice2Param, choice3Param, choice4Param, quesScoreParam, correctChoiceNoParam);
            }
            else
            {
                // Just update the question text if it's not MCQ or doesn't have enough choices
                UpdateQuestionText(question.QuesId, question.QuesText);
            }
        }

        // Call stored procedure to get all questions
        public List<Question> GetAllQuestions()
        {
            return _context.Questions
                .FromSqlRaw("EXEC sp_get_all_questions")
                .AsEnumerable()
                .ToList();
        }

        // Call stored procedure to get question choices
        public List<Choice> GetQuestionChoices(int questionId)
        {
            var quesIdParam = new SqlParameter("@QuesID", questionId);

            return _context.Choices
                .FromSqlRaw("EXEC sp_get_choices_quesid @QuesID", quesIdParam)
                .AsEnumerable()
                .ToList();
        }
    }
}
