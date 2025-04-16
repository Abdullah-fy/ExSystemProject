using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    public class QuestionBankController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuestionBankController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: QuestionBank
        public IActionResult Index(int? examId = null)
        {
            List<Question> questions;

            if (examId.HasValue)
            {
                // Get questions for a specific exam
                questions = _unitOfWork.examRepo.GetQuestionsByExamId(examId.Value);
                ViewBag.ExamId = examId;
                ViewBag.ExamName = _unitOfWork.examRepo.GetExamById(examId.Value)?.ExamName;
            }
            else
            {
                // Get all questions
                questions = _unitOfWork.questionRepo.GetAllQuestions();
            }

            var questionDTOs = _mapper.Map<List<QuestionBankDTO>>(questions);
            return View(questionDTOs);
        }

        // GET: QuestionBank/Details/5
        public IActionResult Details(int id)
        {
            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            // Get choices for this question
            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);
            return View(questionDTO);
        }

        // GET: QuestionBank/Create
        public IActionResult Create(int? examId = null)
        {
            var exams = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", examId);
            ViewBag.ExamId = examId;

            return View(new QuestionBankDTO
            {
                ExamId = examId,
                QuesType = "MCQ", // Default type
                Choices = new List<ChoiceDTO>
                {
                    new ChoiceDTO { ChoiceText = "", IsCorrect = false },
                    new ChoiceDTO { ChoiceText = "", IsCorrect = false },
                    new ChoiceDTO { ChoiceText = "", IsCorrect = false },
                    new ChoiceDTO { ChoiceText = "", IsCorrect = false }
                }
            });
        }

        // POST: QuestionBank/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionBankDTO questionDTO)
        {
            if (ModelState.IsValid)
            {
                var question = _mapper.Map<Question>(questionDTO);

                if (questionDTO.QuesType == "MCQ")
                {
                    // Create multiple-choice question
                    var choices = _mapper.Map<List<Choice>>(questionDTO.Choices);
                    int questionId = _unitOfWork.questionRepo.InsertQuestionMCQ(question, choices);
                }
                else if (questionDTO.QuesType == "TF")
                {
                    // Create true/false question
                    bool isTrue = questionDTO.Choices?.FirstOrDefault()?.IsCorrect ?? false;
                    int questionId = _unitOfWork.questionRepo.InsertQuestionTF(question, isTrue);
                }

                if (questionDTO.ExamId.HasValue)
                    return RedirectToAction(nameof(Index), new { examId = questionDTO.ExamId });
                else
                    return RedirectToAction(nameof(Index));
            }

            var exams = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);

            return View(questionDTO);
        }

        // GET: QuestionBank/Edit/5
        public IActionResult Edit(int id)
        {
            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            // Get choices for this question
            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);

            var exams = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", question.ExamId);

            return View(questionDTO);
        }

        // POST: QuestionBank/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuestionBankDTO questionDTO)
        {
            if (id != questionDTO.QuesId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var question = _mapper.Map<Question>(questionDTO);
                var choices = _mapper.Map<List<Choice>>(questionDTO.Choices);

                // Update question and its choices
                _unitOfWork.questionRepo.UpdateQuestionAndChoices(question, choices);

                if (questionDTO.ExamId.HasValue)
                    return RedirectToAction(nameof(Index), new { examId = questionDTO.ExamId });
                else
                    return RedirectToAction(nameof(Index));
            }

            var exams = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);

            return View(questionDTO);
        }

        // GET: QuestionBank/Delete/5
        public IActionResult Delete(int id)
        {
            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            // Get choices for this question
            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);
            return View(questionDTO);
        }
        // POST: QuestionBank/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var question = _unitOfWork.questionRepo.getById(id);
                int? examId = question?.ExamId;

                // Delete question
                _unitOfWork.questionRepo.DeleteQuestion(id);

                TempData["Success"] = true;
                TempData["Message"] = "Question deleted successfully";

                if (examId.HasValue)
                    return RedirectToAction(nameof(Index), new { examId = examId });
                else
                    return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Handle the foreign key constraint error
                TempData["Error"] = true;
                TempData["Message"] = "Cannot delete this question because it has student answers associated with it.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: QuestionBank/AddToExam/5
        public IActionResult AddToExam(int id)
        {
            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            // Get exams for dropdown (excluding the one the question is already in)
            var exams = _unitOfWork.examRepo.GetAllExams()
                .Where(e => e.ExamId != question.ExamId)
                .ToList();

            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName");

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);
            return View(questionDTO);
        }

        // POST: QuestionBank/AddToExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToExam(int id, int examId)
        {
            // Add question to exam
            _unitOfWork.examRepo.AddQuestionToExam(examId, id);

            return RedirectToAction(nameof(Index), new { examId = examId });
        }

        // POST: QuestionBank/RemoveFromExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromExam(int id, int examId)
        {
            // Remove question from exam
            _unitOfWork.examRepo.RemoveQuestionFromExam(examId, id);

            return RedirectToAction(nameof(Index), new { examId = examId });
        }
    }
}
