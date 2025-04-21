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
            if (choices.Count < 2)
                throw new ArgumentException("MCQ questions require at least 2 choices");

            // Find the correct choice index (1-based)
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
            var choice3Param = new SqlParameter("@choice3", choices.Count > 2 ? choices[2].ChoiceText : "N/A");
            var choice4Param = new SqlParameter("@choice4", choices.Count > 3 ? choices[3].ChoiceText : "N/A");
            var quesScoreParam = new SqlParameter("@ques_score", question.QuesScore);
            var correctChoiceNoParam = new SqlParameter("@correct_choice_no", correctChoiceNo);
            var examIdParam = new SqlParameter("@exam_id", question.ExamId ?? (object)DBNull.Value);

            try
            {
                // Execute stored procedure
                var result = _context.Questions
                    .FromSqlRaw("EXEC sp_insert_ques_mcq @ques_text, @choice1, @choice2, @choice3, @choice4, @ques_score, @correct_choice_no, @exam_id",
                        quesTextParam, choice1Param, choice2Param, choice3Param, choice4Param, quesScoreParam, correctChoiceNoParam, examIdParam)
                    .AsEnumerable()
                    .FirstOrDefault();

                return result?.QuesId ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting MCQ question: {ex.Message}", ex);
            }
        }




        // Repository Implementation
        public int InsertQuestionTF(Question question, string correctAnswer)
        {
            // Always set questions to active when creating
            question.Isactive = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Repository: Inserting TF question with answer={correctAnswer}");

                var quesTextParam = new SqlParameter("@ques_text", question.QuesText);
                var quesScoreParam = new SqlParameter("@ques_score", question.QuesScore);

                // Ensure correctAnswer is "1" or "0" for the stored procedure
                var correctAnswerParam = new SqlParameter("@correct_answer", SqlDbType.VarChar, 20)
                {
                    Value = correctAnswer // Should be "1" or "0"
                };

                var examIdParam = new SqlParameter("@exam_id", question.ExamId ?? (object)DBNull.Value);

                System.Diagnostics.Debug.WriteLine($"Repository: Parameters - Text='{question.QuesText}', Score={question.QuesScore}, Answer='{correctAnswer}', ExamId={question.ExamId ?? (object)DBNull.Value}");

                // Try a different approach to call the stored procedure
                try
                {
                    // Try direct SQL execution with explicit parameter values to debug
                    var sql = $"EXEC sp_insert_ques_tf @ques_text='{question.QuesText.Replace("'", "''")}', @ques_score={question.QuesScore}, @correct_answer='{correctAnswer}', @exam_id={question.ExamId ?? (object)DBNull.Value}";
                    System.Diagnostics.Debug.WriteLine($"Repository: Executing SQL: {sql}");

                    // Use the normal method for actual execution
                    var result = _context.Questions
                        .FromSqlRaw("EXEC sp_insert_ques_tf @ques_text, @ques_score, @correct_answer, @exam_id",
                            quesTextParam, quesScoreParam, correctAnswerParam, examIdParam)
                        .AsEnumerable()
                        .FirstOrDefault();

                    int resultId = result?.QuesId ?? 0;
                    System.Diagnostics.Debug.WriteLine($"Repository: Result ID = {resultId}");

                    if (resultId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Repository: No question ID returned or ID was 0");
                        // Try to get more information about why no result was returned
                        var testQuery = _context.Questions.Where(q => q.QuesText == question.QuesText).FirstOrDefault();
                        if (testQuery != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Repository: Found matching question with ID {testQuery.QuesId}");
                            return testQuery.QuesId;
                        }
                    }

                    return resultId;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Repository: SQL execution error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Repository: Stack trace: {ex.StackTrace}");

                    // Try a fallback approach - direct SQL without using FromSqlRaw
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Repository: Trying fallback approach with direct SQL");

                        // Insert the question directly with SQL
                        var insertSql = $"INSERT INTO Question (ques_text, ques_type, ques_score, exam_id, Isactive) VALUES (@p0, @p1, @p2, @p3, @p4); SELECT SCOPE_IDENTITY();";
                        var insertParams = new object[] {
                    question.QuesText,
                    "True/False",
                    question.QuesScore,
                    question.ExamId ?? (object)DBNull.Value,
                    true
                };

                        var quesId = _context.Database.ExecuteSqlRaw(insertSql, insertParams);
                        System.Diagnostics.Debug.WriteLine($"Repository: Direct insert result: {quesId}");

                        if (quesId > 0)
                        {
                            // Now insert the choices
                            var choiceSql = $"INSERT INTO Choice (ques_id, choice_text, is_correct) VALUES (@p0, 'True', @p1), (@p0, 'False', @p2)";
                            var choiceParams = new object[] {
                        quesId,
                        correctAnswer == "1" ? 1 : 0,
                        correctAnswer == "0" ? 1 : 0
                    };

                            _context.Database.ExecuteSqlRaw(choiceSql, choiceParams);
                            System.Diagnostics.Debug.WriteLine($"Repository: Inserted choices for question {quesId}");

                            // Update exam total marks if needed
                            if (question.ExamId.HasValue)
                            {
                                var updateSql = "UPDATE Exam SET TotalMarks += @p0 WHERE exam_id = @p1";
                                var updateParams = new object[] { question.QuesScore, question.ExamId.Value };
                                _context.Database.ExecuteSqlRaw(updateSql, updateParams);
                            }

                            return quesId;
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Repository: Fallback approach error: {fallbackEx.Message}");
                    }

                    throw; // Re-throw the original exception if all approaches fail
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Repository: Error in InsertQuestionTF: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Repository: Stack trace: {ex.StackTrace}");
                throw new Exception($"Error inserting True/False question: {ex.Message}", ex);
            }
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
            catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint violation
            {
                throw new Exception($"Cannot delete question with ID {questionId}. It has student answers associated with it.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting question: {ex.Message}", ex);
            }
        }

        // Call stored procedure to update question text
        public void UpdateQuestionText(int questionId, string questionText)
        {
            try
            {
                var quesIdParam = new SqlParameter("@ques_id", questionId);
                var quesTextParam = new SqlParameter("@ques_text", questionText);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_update_question_text @ques_id, @ques_text",
                    quesIdParam, quesTextParam);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating question text: {ex.Message}", ex);
            }
        }

        public void UpdateQuestionAndChoices(Question question, List<Choice> choices)
        {
            try
            {
                if (question.QuesType == "MCQ" && choices.Count >= 2)
                {
                    // Find the correct choice index (1-based)
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

                    // Ensure we have at least 4 choices for the procedure
                    while (choices.Count < 4)
                    {
                        choices.Add(new Choice { ChoiceText = "N/A", IsCorrect = false });
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
                else if (question.QuesType == "TF" && choices.Count >= 2)
                {
                    // For T/F questions, find if "True" is the correct answer
                    bool isTrue = choices.Any(c => c.IsCorrect && c.ChoiceText.ToLower() == "true");

                    // Update question using MCQ update since the database doesn't have a specific T/F update procedure
                    var quesIdParam = new SqlParameter("@ques_id", question.QuesId);
                    var quesTextParam = new SqlParameter("@ques_text", question.QuesText);
                    var choice1Param = new SqlParameter("@choice1", "True");
                    var choice2Param = new SqlParameter("@choice2", "False");
                    var choice3Param = new SqlParameter("@choice3", "N/A");
                    var choice4Param = new SqlParameter("@choice4", "N/A");
                    var quesScoreParam = new SqlParameter("@ques_score", question.QuesScore);
                    var correctChoiceNoParam = new SqlParameter("@correct_choice_no", isTrue ? 1 : 2);

                    _context.Database.ExecuteSqlRaw(
                        "EXEC sp_update_question_and_choices @ques_id, @ques_text, @choice1, @choice2, @choice3, @choice4, @ques_score, @correct_choice_no",
                        quesIdParam, quesTextParam, choice1Param, choice2Param, choice3Param, choice4Param, quesScoreParam, correctChoiceNoParam);
                }
                else
                {
                    // Just update the question text if it doesn't match known types or has insufficient choices
                    UpdateQuestionText(question.QuesId, question.QuesText);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating question and choices: {ex.Message}", ex);
            }
        }

        // Call stored procedure to get all questions
        public List<Question> GetAllQuestions()
        {
            try
            {
                return _context.Questions
                    .FromSqlRaw("EXEC sp_get_all_questions")
                    .AsEnumerable()
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving questions: {ex.Message}", ex);
            }
        }

        // Call stored procedure to get question choices
        public List<Choice> GetQuestionChoices(int questionId)
        {
            try
            {
                var quesIdParam = new SqlParameter("@QuesID", questionId);

                return _context.Choices
                    .FromSqlRaw("EXEC sp_get_choices_quesid @QuesID", quesIdParam)
                    .AsEnumerable()
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving choices for question {questionId}: {ex.Message}", ex);
            }
        }
    }
}
