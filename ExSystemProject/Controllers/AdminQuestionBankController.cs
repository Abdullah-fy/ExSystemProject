using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    public class AdminQuestionBankController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminQuestionBankController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: AdminQuestionBank
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

        // GET: AdminQuestionBank/Details/5
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

        // GET: AdminQuestionBank/Create
        public IActionResult Create(int? examId = null)
        {
            var exams = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", examId);
            ViewBag.ExamId = examId;

            return View(new QuestionBankDTO
            {
                ExamId = examId,
                QuesScore = 5, // Default score
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

        // POST: AdminQuestionBank/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionBankDTO questionDTO, string tfCorrectAnswer, string correctAnswer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var question = new Question
                    {
                        QuesText = questionDTO.QuesText,
                        QuesType = questionDTO.QuesType,
                        ExamId = questionDTO.ExamId,
                        QuesScore = questionDTO.QuesScore,
                        Isactive = true
                    };

                    int questionId = 0;

                    if (questionDTO.QuesType == "MCQ")
                    {
                        // Process the correctAnswer parameter if provided
                        if (!string.IsNullOrEmpty(correctAnswer) && int.TryParse(correctAnswer, out int correctIndex) &&
                            correctIndex >= 0 && correctIndex < questionDTO.Choices.Count)
                        {
                            // Mark the selected choice as correct
                            for (int i = 0; i < questionDTO.Choices.Count; i++)
                            {
                                questionDTO.Choices[i].IsCorrect = (i == correctIndex);
                            }
                        }

                        // Check if one of the choices is marked as correct
                        bool hasCorrectAnswer = questionDTO.Choices.Any(c => c.IsCorrect);

                        if (!hasCorrectAnswer)
                        {
                            ModelState.AddModelError("", "You must select one correct answer for the multiple choice question.");
                            var exams = _unitOfWork.examRepo.GetAllExams();
                            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);
                            ViewBag.ExamId = questionDTO.ExamId;
                            return View(questionDTO);
                        }

                        var choices = new List<Choice>();
                        foreach (var choiceDto in questionDTO.Choices)
                        {
                            // Skip empty choices
                            if (!string.IsNullOrWhiteSpace(choiceDto.ChoiceText))
                            {
                                choices.Add(new Choice
                                {
                                    ChoiceText = choiceDto.ChoiceText,
                                    IsCorrect = choiceDto.IsCorrect
                                });
                            }
                        }

                        if (choices.Count < 2)
                        {
                            ModelState.AddModelError("", "MCQ questions require at least 2 choices.");
                            var exams = _unitOfWork.examRepo.GetAllExams();
                            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);
                            ViewBag.ExamId = questionDTO.ExamId;
                            return View(questionDTO);
                        }

                        // Pad with empty choices if less than 4 are provided
                        while (choices.Count < 4)
                        {
                            choices.Add(new Choice
                            {
                                ChoiceText = "N/A",
                                IsCorrect = false
                            });
                        }

                        questionId = _unitOfWork.questionRepo.InsertQuestionMCQ(question, choices);
                    }
                    else if (questionDTO.QuesType == "TF")
                    {
                        // Create true/false question
                        bool isTrue = tfCorrectAnswer == "true";
                        questionId = _unitOfWork.questionRepo.InsertQuestionTF(question, isTrue);
                    }

                    TempData["Success"] = true;
                    TempData["Message"] = "Question created successfully";

                    if (questionDTO.ExamId.HasValue)
                        return RedirectToAction(nameof(Index), new { examId = questionDTO.ExamId });
                    else
                        return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            var examsList = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName", questionDTO.ExamId);
            ViewBag.ExamId = questionDTO.ExamId;
            return View(questionDTO);
        }

        // GET: AdminQuestionBank/Edit/5
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

        // POST: AdminQuestionBank/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuestionBankDTO questionDTO, string correctAnswer)
        {
            if (id != questionDTO.QuesId)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var question = new Question
                    {
                        QuesId = questionDTO.QuesId,
                        QuesText = questionDTO.QuesText,
                        QuesType = questionDTO.QuesType,
                        ExamId = questionDTO.ExamId,
                        QuesScore = questionDTO.QuesScore,
                        Isactive = questionDTO.Isactive ?? true
                    };

                    var choices = new List<Choice>();
                    bool hasCorrectChoice = false;

                    if (questionDTO.QuesType == "MCQ")
                    {
                        // For MCQ questions, process each choice
                        // If correctAnswer is provided (0-based index), use it to mark the correct answer
                        if (!string.IsNullOrEmpty(correctAnswer) && int.TryParse(correctAnswer, out int correctIndex) &&
                            correctIndex >= 0 && correctIndex < questionDTO.Choices.Count)
                        {
                            for (int i = 0; i < questionDTO.Choices.Count; i++)
                            {
                                questionDTO.Choices[i].IsCorrect = (i == correctIndex);
                            }
                            hasCorrectChoice = true;
                        }
                        else
                        {
                            // Otherwise check if any choice is already marked as correct
                            foreach (var choice in questionDTO.Choices)
                            {
                                if (choice.IsCorrect)
                                {
                                    hasCorrectChoice = true;
                                    break;
                                }
                            }
                        }

                        if (!hasCorrectChoice)
                        {
                            ModelState.AddModelError("", "You must select one correct answer.");
                            var availableExams = _unitOfWork.examRepo.GetAllExams();
                            ViewBag.Exams = new SelectList(availableExams, "ExamId", "ExamName", questionDTO.ExamId);
                            return View(questionDTO);
                        }

                        foreach (var choiceDto in questionDTO.Choices)
                        {
                            choices.Add(new Choice
                            {
                                ChoiceId = choiceDto.ChoiceId,
                                ChoiceText = choiceDto.ChoiceText,
                                QuesId = questionDTO.QuesId,
                                IsCorrect = choiceDto.IsCorrect
                            });
                        }
                    }
                    else if (questionDTO.QuesType == "TF")
                    {
                        // For True/False questions, determine which option is correct
                        bool isTrue = false;

                        if (!string.IsNullOrEmpty(correctAnswer))
                        {
                            isTrue = correctAnswer.ToLower() == "true";
                            hasCorrectChoice = true;
                        }
                        else
                        {
                            isTrue = questionDTO.Choices.Any(c => c.IsCorrect && c.ChoiceText.ToLower() == "true");
                            hasCorrectChoice = true;
                        }

                        // Create two choices for T/F question
                        choices = new List<Choice>
                        {
                            new Choice
                            {
                                ChoiceId = questionDTO.Choices.FirstOrDefault(c => c.ChoiceText.ToLower() == "true")?.ChoiceId ?? 0,
                                ChoiceText = "True",
                                QuesId = questionDTO.QuesId,
                                IsCorrect = isTrue
                            },
                            new Choice
                            {
                                ChoiceId = questionDTO.Choices.FirstOrDefault(c => c.ChoiceText.ToLower() == "false")?.ChoiceId ?? 0,
                                ChoiceText = "False",
                                QuesId = questionDTO.QuesId,
                                IsCorrect = !isTrue
                            }
                        };
                    }

                    // Update question and its choices
                    _unitOfWork.questionRepo.UpdateQuestionAndChoices(question, choices);

                    TempData["Success"] = true;
                    TempData["Message"] = "Question updated successfully";

                    if (questionDTO.ExamId.HasValue)
                        return RedirectToAction(nameof(Index), new { examId = questionDTO.ExamId });
                    else
                        return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            // Use a different variable name for exams list to avoid name conflict
            var examOptions = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examOptions, "ExamId", "ExamName", questionDTO.ExamId);
            return View(questionDTO);
        }

        // GET: AdminQuestionBank/Delete/5
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

        // POST: AdminQuestionBank/Delete/5
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

        // GET: AdminQuestionBank/AddToExam/5
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

        // POST: AdminQuestionBank/AddToExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToExam(int id, int examId)
        {
            try
            {
                // Add question to exam
                _unitOfWork.examRepo.AddQuestionToExam(examId, id);

                TempData["Success"] = true;
                TempData["Message"] = "Question successfully added to exam";

                return RedirectToAction(nameof(Index), new { examId = examId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = true;
                TempData["Message"] = $"Error adding question to exam: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminQuestionBank/RemoveFromExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromExam(int id, int examId)
        {
            try
            {
                // Remove question from exam
                _unitOfWork.examRepo.RemoveQuestionFromExam(examId, id);

                TempData["Success"] = true;
                TempData["Message"] = "Question removed from exam successfully";

                return RedirectToAction(nameof(Index), new { examId = examId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = true;
                TempData["Message"] = $"Error removing question from exam: {ex.Message}";

                return RedirectToAction(nameof(Index), new { examId = examId });
            }
        }
    }
}
