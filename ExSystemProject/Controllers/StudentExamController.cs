using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    public class StudentExamController : Controller
    {
        private readonly UnitOfWork unitOfWork;

        public StudentExamController(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult getassignexamtostudent()
        {
            // get studentid from cookie 
            // get track id from cookie 
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            // checking first if student login or not 
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized(); // should redirect to login page 

            var userid = userclaim.Value;

            //var trackid = Request.Cookies["TrackId"];

            //if (string.IsNullOrEmpty(trackid))
            //    return Unauthorized(); // or redirect to login
            

            var std = unitOfWork.studentRepo.Getstd(Convert.ToInt32(userid));
            if (std == null || std.Track == null)
                return NotFound();

            List<GetAssignExamToStudentDTO> listo = unitOfWork.studentExamRepo.GetAssignExamToStudent(std.StudentId);

           
            return View(listo);
        }

        public IActionResult JoinExam(int examid)
        {
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            // checking first if student login or not 
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized(); // should redirect to login page 

            var userid = userclaim.Value;

            //var trackid = Request.Cookies["TrackId"];

            //if (string.IsNullOrEmpty(trackid))
            //    return Unauthorized(); // or redirect to login
            // hello 

            var std = unitOfWork.studentRepo.Getstd(Convert.ToInt32(userid));
            if (std == null || std.Track == null)
                return NotFound();
            var questions = unitOfWork.studentExamRepo.GetExamQuestionsAndChoices(examid);
            ViewBag.examid = examid;
            var examm = unitOfWork.examRepo.getexambyid(examid);
            ViewBag.startexam = examm.StartTime;
            ViewBag.endexam = examm.EndTime;
            ViewBag.studentid = std.StudentId;
            return View(questions);
        }


        //[HttpPost("api/exam/submit-all")]
        //public IActionResult SubmitAnswers([FromBody] List<SubmitAnswerDTO> answers)
        //{
        //    foreach (var answer in answers)
        //    {
        //        unitOfWork.studentExamRepo.SubmitExamAnswer(answer);
        //    }

        //    return Ok(new { message = "All answers submitted successfully." });
        //}
        [HttpPost("api/exam/submit-all")]
        public IActionResult SubmitAnswers([FromBody] List<SubmitAnswerDTO> answers)
        {
            try
            {
                // Get student and exam IDs from the first answer (they should be the same for all)
                int studentId = answers.FirstOrDefault()?.StudentId ?? 0;
                int examId = answers.FirstOrDefault()?.ExamId ?? 0;

                if (studentId == 0 || examId == 0)
                {
                    return BadRequest("Missing student or exam information");
                }

                // Filter out null answers and submit only answered questions
                var answeredQuestions = answers.Where(a => a.ChoiceId != null).ToList();

                foreach (var answer in answeredQuestions)
                {
                    unitOfWork.studentExamRepo.SubmitExamAnswer(answer);
                }

                // Deactivate the exam regardless of how many answers were submitted
                unitOfWork.studentExamRepo.DeactivateStudentExam(studentId, examId);

                return Ok(new
                {
                    message = $"Submitted {answeredQuestions.Count} answers successfully",
                    submittedCount = answeredQuestions.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error submitting answers: " + ex.Message });
            }
        }

        [HttpPost("api/exam/deactivate")]
        public async Task<IActionResult> DeactivateExam([FromBody] DeactivateexamtostudentDTO request)
        {
            unitOfWork.studentExamRepo.DeactivateStudentExam(request.StudentID, request.ExamID);
            return Ok(new { message = "Exam deactivated successfully." });
        }

        public async Task<IActionResult> ResultsofExams()
        {
            var userclaim = User.FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            if (userclaim == null || string.IsNullOrEmpty(userclaim.Value))
                return Unauthorized();

            var userid = userclaim.Value;
            var std = unitOfWork.studentRepo.Getstd(Convert.ToInt32(userid));
            if (std == null || std.Track == null)
                return NotFound();

            var exams = unitOfWork.studentExamRepo.GetAssignExamToStudent(std.StudentId);
            var results = new List<StudentExamResultsDTO>();

            foreach (var exam in exams)
            {
                var examResults = await unitOfWork.studentExamRepo.GetStudentExamResultsAsync(exam.ExamID, std.StudentId);
                if (examResults != null)
                {
                    results.AddRange(examResults);
                }
            }

            return View(results);
        }


        [HttpPost("api/exam/submit-empty")]
        public IActionResult SubmitEmptyExam([FromBody] EmptyExamSubmissionDTO request)
        {
            try
            {
                // Submit empty exam with score 0
                string result = unitOfWork.studentExamRepo.SubmitEmptyExam(request.StudentId, request.ExamId);

                // Deactivate the exam
                unitOfWork.studentExamRepo.DeactivateStudentExam(request.StudentId, request.ExamId);

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error submitting empty exam: " + ex.Message });
            }
        }

        // Add this DTO class somewhere in your project
       
    }
}
