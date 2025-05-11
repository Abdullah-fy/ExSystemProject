using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ExSystemProject.Repository
{
    public class ChoicesRepo : GenaricRepo<Choice>
    {
        private readonly ExSystemTestContext _context;

        public ChoicesRepo(ExSystemTestContext context) : base(context)
        {
            _context = context;
        }

        public void AddChoiceToQuestion(int questionId, string choiceText, bool isCorrect)
        {
            var quesIdParam = new SqlParameter("@QuesId", questionId);
            var choiceTextParam = new SqlParameter("@ChoiceText", choiceText);
            var isCorrectParam = new SqlParameter("@IsCorrect", isCorrect);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_add_choice_to_ques @QuesId, @ChoiceText, @IsCorrect",
                quesIdParam, choiceTextParam, isCorrectParam);
        }

        public void RemoveChoiceFromQuestion(int choiceId)
        {
            var choiceIdParam = new SqlParameter("@ChoiceID", choiceId);

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_remove_choice_from_ques @ChoiceID",
                choiceIdParam);
        }

        public List<Choice> GetChoicesByQuestionId(int questionId)
        {
            var quesIdParam = new SqlParameter("@QuesId", questionId);

            return _context.Choices
                .FromSqlRaw("EXEC sp_get_choices_quesid @QuesId", quesIdParam)
                .AsEnumerable()
                .ToList();
        }
        

        public Choice GetCorrectChoiceForQuestion(int questionId)
        {
            return _context.Choices
                .Where(c => c.QuesId == questionId && c.IsCorrect)
                .FirstOrDefault();
        }

        public void UpdateChoice(Choice choice)
        {
            try
            {
                _context.Entry(choice).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating choice: {ex.Message}", ex);
            }
        }

        public void AddChoices(List<Choice> choices)
        {
            try
            {
                _context.Choices.AddRange(choices);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding choices: {ex.Message}", ex);
            }
        }

    }
}
