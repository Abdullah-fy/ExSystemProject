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
    [Authorize(Roles = "admin")]
    public class BranchManagerQuestionBankController : BranchManagerBaseController
    {
        private readonly IMapper _mapper;

        public BranchManagerQuestionBankController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: BranchManagerQuestionBank
        public IActionResult Index(int? examId = null)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            List<Question> questions;
            ViewBag.BranchId = branchId;

            if (examId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(examId.Value);

                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "Exam not found or you don't have access to it.";
                    return RedirectToAction(nameof(Index));
                }

                questions = _unitOfWork.examRepo.GetQuestionsByExamId(examId.Value);
                ViewBag.ExamId = examId;
                ViewBag.ExamName = exam.ExamName;
            }
            else
            {
                questions = GetBranchQuestions(branchId.Value);
            }

            var questionDTOs = _mapper.Map<List<QuestionBankDTO>>(questions);
            return View(questionDTOs);
        }

        private List<Question> GetBranchQuestions(int branchId)
        {
            var branchCourses = _unitOfWork.courseRepo.GetCoursesByBranch(branchId);

            if (branchCourses == null || !branchCourses.Any())
                return new List<Question>();

            var exams = new List<Exam>();
            foreach (var course in branchCourses)
            {
                var courseExams = _unitOfWork.examRepo.GetExamsByCourseId(course.CrsId);
                if (courseExams != null && courseExams.Any())
                {
                    exams.AddRange(courseExams);
                }
            }

            if (!exams.Any())
                return new List<Question>();

            var questions = new List<Question>();
            foreach (var exam in exams)
            {
                var examQuestions = _unitOfWork.examRepo.GetQuestionsByExamId(exam.ExamId);
                if (examQuestions != null && examQuestions.Any())
                {
                    questions.AddRange(examQuestions);
                }
            }

            return questions;
        }

       
        private bool IsCourseInBranch(int? courseId, int branchId)
        {
            if (!courseId.HasValue)
                return false;

            var course = _unitOfWork.courseRepo.GetCourseWithBranch(courseId.Value);
            return course != null && course.Ins?.Track?.BranchId == branchId;
        }

        // GET: BranchManagerQuestionBank/Details/5
        public IActionResult Details(int id)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            if (question.ExamId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(question.ExamId.Value);
                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "Question not found or you don't have access to it.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData["Error"] = "Question not assigned to any exam.";
                return RedirectToAction(nameof(Index));
            }

            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);
            return View(questionDTO);
        }

        // GET: BranchManagerQuestionBank/Create
        public IActionResult Create(int? examId = null)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            var branchExams = GetBranchExams(branchId.Value);
            ViewBag.Exams = new SelectList(branchExams, "ExamId", "ExamName", examId);
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

       
        private List<Exam> GetBranchExams(int branchId)
        {
            var branchCourses = _unitOfWork.courseRepo.GetCoursesByBranch(branchId);

            if (branchCourses == null || !branchCourses.Any())
                return new List<Exam>();

            var exams = new List<Exam>();
            foreach (var course in branchCourses)
            {
                var courseExams = _unitOfWork.examRepo.GetExamsByCourseId(course.CrsId);
                if (courseExams != null && courseExams.Any())
                {
                    exams.AddRange(courseExams);
                }
            }

            return exams;
        }

        // POST: BranchManagerQuestionBank/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionBankDTO questionDTO)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            if (questionDTO.ExamId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(questionDTO.ExamId.Value);
                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "You don't have access to this exam.";
                    return RedirectToAction(nameof(Index));
                }
            }

            try
            {
                questionDTO.QuesType = "MCQ";

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
                        var exams = GetBranchExams(branchId.Value);
                        ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", questionDTO.ExamId);
                        ViewBag.ExamId = questionDTO.ExamId;
                        return View(questionDTO);
                    }

                    if (choices.Count < 2)
                    {
                        ModelState.AddModelError("", "MCQ questions must have at least 2 choices.");
                        var exams = GetBranchExams(branchId.Value);
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

                    questionId = _unitOfWork.questionRepo.InsertQuestionMCQDirect(question, choices);

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
                ModelState.AddModelError("", $"Error creating MCQ question: {ex.Message}");
            }

            var examsList = GetBranchExams(branchId.Value);
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName", questionDTO.ExamId);
            ViewBag.ExamId = questionDTO.ExamId;

            return View(questionDTO);
        }


        // GET: BranchManagerQuestionBank/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            if (question.ExamId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(question.ExamId.Value);
                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "Question not found or you don't have access to it.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData["Error"] = "Question not assigned to any exam.";
                return RedirectToAction(nameof(Index));
            }

            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);

            var exams = GetBranchExams(branchId.Value);
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", question.ExamId);

            return View(questionDTO);
        }

        // POST: BranchManagerQuestionBank/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuestionBankDTO questionDTO, string correctAnswer)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            if (id != questionDTO.QuesId)
                return NotFound();

            if (questionDTO.ExamId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(questionDTO.ExamId.Value);
                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "You don't have access to this exam.";
                    return RedirectToAction(nameof(Index));
                }
            }

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
                            var availableExams = GetBranchExams(branchId.Value);
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

            var examOptions = GetBranchExams(branchId.Value);
            ViewBag.Exams = new SelectList(examOptions, "ExamId", "ExamName", questionDTO.ExamId);
            return View(questionDTO);
        }

        // GET: BranchManagerQuestionBank/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            var question = _unitOfWork.questionRepo.getById(id);
            if (question == null)
                return NotFound();

            if (question.ExamId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(question.ExamId.Value);
                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "Question not found or you don't have access to it.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData["Error"] = "Question not assigned to any exam.";
                return RedirectToAction(nameof(Index));
            }

            var choices = _unitOfWork.choicesRepo.GetChoicesByQuestionId(id);
            question.Choices = choices;

            var questionDTO = _mapper.Map<QuestionBankDTO>(question);
            return View(questionDTO);
        }

        // POST: BranchManagerQuestionBank/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            try
            {
                var question = _unitOfWork.questionRepo.getById(id);
                if (question == null)
                    return NotFound();

                if (question.ExamId.HasValue)
                {
                    var exam = _unitOfWork.examRepo.GetExamById(question.ExamId.Value);
                    if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                    {
                        TempData["Error"] = "Question not found or you don't have access to it.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                int? examId = question.ExamId;

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

        // GET: BranchManagerQuestionBank/CreateTrueFalse
        [HttpGet]
        public IActionResult CreateTrueFalse()
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            var examsList = GetBranchExams(branchId.Value);
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName");
            return View();
        }

        // POST: BranchManagerQuestionBank/CreateTrueFalse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTrueFalse(string quesText, int quesScore, string tfCorrectAnswer, int? examId)
        {
            var userId = GetCurrentUserId();
            var branchId = GetUserBranchId();

            if (branchId == null)
            {
                TempData["Error"] = "No branch assignment found for this account.";
                return RedirectToAction("Index", "BranchManagerDashboard");
            }

            if (examId.HasValue)
            {
                var exam = _unitOfWork.examRepo.GetExamById(examId.Value);
                if (exam == null || !IsCourseInBranch(exam.CrsId, branchId.Value))
                {
                    TempData["Error"] = "You don't have access to this exam.";
                    return RedirectToAction(nameof(Index));
                }
            }

            try
            {
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
                ModelState.AddModelError("", $"Error creating True/False question: {ex.Message}");
            }

            var examsList = GetBranchExams(branchId.Value);
            ViewBag.Exams = new SelectList(examsList, "ExamId", "ExamName", examId);

            ViewBag.QuesText = quesText;
            ViewBag.QuesScore = quesScore;
            ViewBag.TFCorrectAnswer = tfCorrectAnswer;

            return View();
        }

        private int? GetUserBranchId()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return null;

            var userAssignment = _unitOfWork.userAssignmentRepo.GetUserBranchAssignment(userId.Value);
            return userAssignment?.BranchId;
        }

        private int? GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            return null;
        }
    }
}
