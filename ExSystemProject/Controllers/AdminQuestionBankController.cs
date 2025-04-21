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
        // Controller Action
        // Controller Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionBankDTO questionDTO, string tfCorrectAnswer)
        {
            // Add form debugging
            var form = Request.Form;
            System.Diagnostics.Debug.WriteLine("--- FORM DATA ---");
            foreach (var key in form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"{key}: {form[key]}");
            }
            System.Diagnostics.Debug.WriteLine("-----------------");

            // Debug model binding
            System.Diagnostics.Debug.WriteLine($"QuesText from model: {questionDTO.QuesText}");
            System.Diagnostics.Debug.WriteLine($"QuesType from model: {questionDTO.QuesType}");
            System.Diagnostics.Debug.WriteLine($"QuesScore from model: {questionDTO.QuesScore}");
            System.Diagnostics.Debug.WriteLine($"tfCorrectAnswer from parameter: {tfCorrectAnswer}");

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

                    if (questionDTO.QuesType == "TF")
                    {
                        // Try to get tfCorrectAnswer directly from form if it's not coming through parameter
                        if (string.IsNullOrEmpty(tfCorrectAnswer) && form.ContainsKey("tfCorrectAnswer"))
                        {
                            tfCorrectAnswer = form["tfCorrectAnswer"].ToString();
                            System.Diagnostics.Debug.WriteLine($"tfCorrectAnswer from form: {tfCorrectAnswer}");
                        }

                        // Convert to expected format
                        string correctAnswerValue = (tfCorrectAnswer?.ToLower() == "true") ? "1" : "0";
                        System.Diagnostics.Debug.WriteLine($"Controller: TF answer converted to '{correctAnswerValue}'");

                        // Log properties of the question object
                        System.Diagnostics.Debug.WriteLine($"Controller: Question object - Text='{question.QuesText}', Type='{question.QuesType}', Score={question.QuesScore}, ExamId={question.ExamId}, Active={question.Isactive}");

                        try
                        {
                            questionId = _unitOfWork.questionRepo.InsertQuestionTF(question, correctAnswerValue);
                            System.Diagnostics.Debug.WriteLine($"Controller: Repository returned questionId: {questionId}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Controller: Repository exception: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Controller: Stack trace: {ex.StackTrace}");
                            // Try to get inner exception details
                            if (ex.InnerException != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Controller: Inner exception: {ex.InnerException.Message}");
                            }
                            throw;
                        }
                    }
                    else // MCQ
                    {
                        // Process MCQ choices
                        // ...
                    }

                    if (questionId > 0)
                    {
                        TempData["Success"] = true;
                        TempData["Message"] = $"Question created successfully.";

                        if (questionDTO.ExamId.HasValue)
                            return RedirectToAction("Index", new { examId = questionDTO.ExamId });
                        else
                            return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create question. Question ID was not returned.");
                        System.Diagnostics.Debug.WriteLine("Question ID was 0 or negative");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ModelState is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error creating question: {ex.Message}");
            }

            var examsList = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName", questionDTO.ExamId);
            ViewBag.ExamId = questionDTO.ExamId;

            // IMPORTANT: Send back the values that were submitted
            ViewBag.SubmittedQuesText = questionDTO.QuesText;
            ViewBag.SubmittedQuesScore = questionDTO.QuesScore;
            ViewBag.SubmittedTfCorrectAnswer = tfCorrectAnswer;

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
