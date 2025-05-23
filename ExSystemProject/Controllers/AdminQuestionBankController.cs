﻿using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminQuestionBankController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminQuestionBankController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminQuestionBank
        public IActionResult Index(int? examId = null)
        {
            var userId = GetCurrentUserId();

            List<Question> questions;

            if (examId.HasValue)
            {
                questions = _unitOfWork.examRepo.GetQuestionsByExamId(examId.Value);
                ViewBag.ExamId = examId;
                ViewBag.ExamName = _unitOfWork.examRepo.GetExamById(examId.Value)?.ExamName;
            }
            else
            {
                questions = _unitOfWork.questionRepo.GetAllQuestions();
            }

            var questionDTOs = _mapper.Map<List<QuestionBankDTO>>(questions);
            return View(questionDTOs);
        }

        // GET: AdminQuestionBank/Details/5
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);
            return View(questionDTO);
        }

        // GET: AdminQuestionBank/Create
        public IActionResult Create(int? examId = null)
        {
            var userId = GetCurrentUserId();

            var exams = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", examId);
            ViewBag.ExamId = examId;

            return View(new QuestionBankDTO
            {
                ExamId = examId,
                QuesScore = 5, 
                QuesType = "MCQ", 
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
        public IActionResult Create(QuestionBankDTO questionDTO)
        {
            var userId = GetCurrentUserId();

            try
            {
                questionDTO.QuesType = "MCQ";

                System.Diagnostics.Debug.WriteLine($"Create MCQ POST: Text={questionDTO.QuesText}, Score={questionDTO.QuesScore}");

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

                    var choices = new List<Choice>();
                    bool hasCorrectChoice = false;

                    foreach (var choice in questionDTO.Choices.Where(c => !string.IsNullOrWhiteSpace(c.ChoiceText)))
                    {
                        choices.Add(new Choice
                        {
                            ChoiceText = choice.ChoiceText,
                            IsCorrect = choice.IsCorrect
                        });

                        if (choice.IsCorrect)
                        {
                            hasCorrectChoice = true;
                        }
                    }

                    if (!hasCorrectChoice)
                    {
                        ModelState.AddModelError("", "You must select one correct answer for the MCQ question.");
                        var exams = _unitOfWork.examRepo.GetAllExams();
                        ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);
                        ViewBag.ExamId = questionDTO.ExamId;
                        return View(questionDTO);
                    }

                    if (choices.Count < 2)
                    {
                        ModelState.AddModelError("", "MCQ questions must have at least 2 choices.");
                        var exams = _unitOfWork.examRepo.GetAllExams();
                        ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);
                        ViewBag.ExamId = questionDTO.ExamId;
                        return View(questionDTO);
                    }

                    while (choices.Count < 4)
                    {
                        choices.Add(new Choice
                        {
                            ChoiceText = "N/A",
                            IsCorrect = false
                        });
                    }

                    questionId = _unitOfWork.questionRepo.InsertQuestionMCQ(question, choices);

                    if (questionId > 0)
                    {
                        TempData["Success"] = true;
                        TempData["Message"] = $"MCQ question created successfully.";

                        if (questionDTO.ExamId.HasValue)
                            return RedirectToAction("Index", new { examId = questionDTO.ExamId });
                        else
                            return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create MCQ question");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create MCQ ERROR: {ex.Message}");
                ModelState.AddModelError("", $"Error creating MCQ question: {ex.Message}");
            }

            var examsList = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName", questionDTO.ExamId);
            ViewBag.ExamId = questionDTO.ExamId;

            return View(questionDTO);
        }

        // GET: AdminQuestionBank/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

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
            var userId = GetCurrentUserId();

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

            var examOptions = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examOptions, "ExamId", "ExamName", questionDTO.ExamId);
            return View(questionDTO);
        }

        // GET: AdminQuestionBank/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

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
            var userId = GetCurrentUserId();

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
                TempData["Error"] = true;
                TempData["Message"] = "Cannot delete this question because it has student answers associated with it.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: AdminQuestionBank/AddToExam/5
        public IActionResult AddToExam(int id)
        {
            var userId = GetCurrentUserId();

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

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
            var userId = GetCurrentUserId();

            try
            {
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
            var userId = GetCurrentUserId();

            try
            {
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

        [HttpGet]
        public IActionResult CreateTrueFalse()
        {
            var userId = GetCurrentUserId();

            var examsList = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTrueFalse(string quesText, int quesScore, string tfCorrectAnswer, int? examId)
        {
            var userId = GetCurrentUserId();

            try
            {
                System.Diagnostics.Debug.WriteLine($"CreateTrueFalse POST: Text={quesText}, Score={quesScore}, Answer={tfCorrectAnswer}, ExamId={examId}");

                var question = new Question
                {
                    QuesText = quesText,
                    QuesType = "TF", 
                    QuesScore = quesScore,
                    ExamId = examId,
                    Isactive = true
                };

                string correctAnswerValue = tfCorrectAnswer?.ToLower() == "true" ? "1" : "0";

                int questionId = _unitOfWork.questionRepo.InsertQuestionTF(question, correctAnswerValue);

                if (questionId > 0)
                {
                    TempData["Success"] = true;
                    TempData["Message"] = "True/False question created successfully.";
                    return RedirectToAction("Index", new { examId = examId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create True/False question");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTrueFalse ERROR: {ex.Message}");
                ModelState.AddModelError("", $"Error creating True/False question: {ex.Message}");
            }

            var examsList = _unitOfWork.examRepo.GetAllExams();
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName", examId);

            ViewBag.QuesText = quesText;
            ViewBag.QuesScore = quesScore;
            ViewBag.TFCorrectAnswer = tfCorrectAnswer;

            return View();
        }
    }
}
