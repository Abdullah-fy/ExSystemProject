using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ExSystemProject.Repository
{
    public class AdminChoicesRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminChoicesRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Add choice to question
        public void AddChoiceToQuestion(int questionId, string choiceText, bool isCorrect)
        {
            var quesIdParam = new SqlParameter("@QuesId", questionId);
            var choiceTextParam = new SqlParameter("@ChoiceText", choiceText);
            var isCorrectParam = new SqlParameter("@IsCorrect", isCorrect);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_add_choice_to_ques @QuesId, @ChoiceText, @IsCorrect",
                quesIdParam, choiceTextParam, isCorrectParam);
        }

        // Remove choice from question
        public void RemoveChoiceFromQuestion(int choiceId)
        {
            var choiceIdParam = new SqlParameter("@ChoiceID", choiceId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_remove_choice_from_ques @ChoiceID",
                choiceIdParam);
        }

        // Get choices by question ID
        public List<Choice> GetChoicesByQuestionId(int questionId)
        {
            var quesIdParam = new SqlParameter("@QuesId", questionId);

            return _context.Choices
                .FromSqlRaw("EXEC sp_get_choices_quesid @QuesId", quesIdParam)
                .AsEnumerable()
                .ToList();
        }
    }
}
