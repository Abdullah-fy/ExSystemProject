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


            var std = unitOfWork.studentRepo.Getstd(Convert.ToInt32(userid));
            if (std == null || std.Track == null)
                return NotFound();
            var questions = unitOfWork.studentExamRepo.GetExamQuestionsAndChoices(examid);
            ViewBag.examid = examid;
            ViewBag.studentid = std.StudentId;
            return View(questions);
        }


        [HttpPost("api/exam/submit-all")]
        public IActionResult SubmitAnswers([FromBody] List<SubmitAnswerDTO> answers)
        {
            foreach (var answer in answers)
            {
                unitOfWork.studentExamRepo.SubmitExamAnswer(answer);
            }

            return Ok(new { message = "All answers submitted successfully." });
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
    }
}
