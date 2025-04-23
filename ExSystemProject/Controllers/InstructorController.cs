using System.Security.Claims;
using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using ExSystemProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;

namespace ExSystemProject.Controllers
{
    public class InstructorController : Controller
    {
        private UnitOfWork unit;

        public IMapper mapper { get; }

        public InstructorController(UnitOfWork _unit, IMapper mapper)
        {
            this.unit = _unit;
            this.mapper = mapper;
        }
        #region commented
        //public IActionResult getAllInstructorStudents(string? search, int page = 1, int pageSize = 10)
        //{
        //    var UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var ins = unit.instructorRepo.getByUserId(UserId);
        //    if (ins == null)
        //    {
        //        return NotFound("Instructor not found");
        //    }
        //    var students = unit.instructorRepo.GetStudentsByInstructor(ins.InsId);
        //    if (students == null || !students.Any())
        //    {
        //        return View(new List<MStudentDTO>());
        //    }
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        students = students.Where(i => i.User.Username.Contains(search) || i.Track.TrackName.Contains(search) || i.User.Email.Contains(search)).ToList();
        //    }
        //    int totalCount = students.Count();
        //    var pages = students.Skip((page - 1) *  pageSize).Take(pageSize).ToList();
        //    var mappedStudent = mapper.Map<List<MStudentDTO>>(pages);

        //    ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        //    ViewBag.CurrentPage = page;
        //    ViewBag.Search = search;
        //    return View(mappedStudent);
        //}
        //[HttpGet]

        //public IActionResult Edit(int id)
        //{
        //    var std = unit.studentRepo.getById(id);
        //    if(std == null)
        //    {
        //        return NotFound();
        //    }
        //    var stud = mapper.Map<MStudentDTO>(std);
        //    return View(stud);
        //}
        //[HttpPost]
        //public IActionResult Edit(MStudentDTO stdDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        Console.WriteLine("ModelState Errors:");
        //        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        //        {
        //            Console.WriteLine(error.ErrorMessage);
        //        }
        //        return View(stdDto);
        //    }
        //    var student = unit.studentRepo.getById(stdDto.id);
        //    if (student == null)
        //    {
        //        return NotFound();
        //    }
        //    if (stdDto.UserId == null) return BadRequest();

        //    mapper.Map(stdDto, student);

        //    student.User.Username = stdDto.Username;
        //    student.User.Email = stdDto.Email;
        //    student.User.Gender = stdDto.Gender;
        //    student.User.Img = stdDto.Image;

        //    mapper.Map(stdDto, student);
        //    unit.studentRepo.update(student);

        //    unit.save();
        //    Console.WriteLine("ddd");
        //    return RedirectToAction("getAllInstructorStudents");
        //}

        //    [HttpGet]
        //    public IActionResult Delete(int id)
        //    {

        //        var x = unit.studentRepo.getById(id);
        //        if(x == null)
        //        {
        //            return NotFound();
        //        }
        //        unit.studentRepo.delete(id);
        //        unit.userRepo.delete((int)x.UserId);
        //        unit.save();
        //        return RedirectToAction("getAllInstructorStudents");
        //    }

        //public IActionResult Details(int id)
        //{
        //    var std = unit.studentRepo.getById(id);

        //    return View();
        //}

        //[HttpGet]
        //public IActionResult CreateStudent()
        //{
        //    var vm = new StudentViewModel();
        //    vm.tracks = unit.trackRepo.getAll().Select(t => new SelectListItem
        //    {
        //        Value = t.TrackId.ToString(),
        //        Text = t.TrackName,
        //    }).ToList();
        //    return View(vm);
        //}
        //[HttpPost]
        //public IActionResult CreateStudent(StudentViewModel student)
        //{
        //    if(student == null)
        //    {
        //        return BadRequest();
        //    }
        //    unit.studentRepo.AddNewStudent(student);
        //    unit.save();
        //    return RedirectToAction("getAllInstructorStudents");
        //}
        #endregion


        public IActionResult Index(int? id)
        {
            var UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ins = unit.instructorRepo.getByUserId(UserId);
            if (ins == null)
            {
                return NotFound("Instructor not found");
            }
            //var st = unit.studentRepo.getByUserId(UserId);
            var courses = unit.courseRepo.GetCoursesByInstructor(ins.InsId);
            List<Student> studs = new List<Student>();
            if (id.HasValue)
            {
                studs = unit.studentRepo.GetStudentByInstructorAndCourse(ins.InsId,  id.Value);
            }

            var mappedStudents = studs.Select(s => new StudentViewModelForInstructor
            {
                UserId = (int)s.UserId,
                username = s.User?.Username,
                email = s.User?.Email,
                image = s.User?.Img,
                trackname = s.Track?.TrackName
            }).ToList();

            var st = new StudentFilterViewModelForInstructor
            {
                selectedCourse = id,
                students = mappedStudents,
                courses = courses
            };
            return View(st);
        }
        
              public IActionResult studentCourses(int userId)
            {
                
                var student = unit.studentRepo.getByUserId(userId);
                var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var instructorId = int.Parse(userClaim.Value);
                var ins = unit.instructorRepo.getByUserId(instructorId);
                if (ins == null)
                    return NotFound("Instructor not found");
                var courses = unit.courseRepo.getCourseByStudentAndInstructor(student.StudentId, ins.InsId);
                ViewBag.courses = courses;
                return View("studentCourses", student);
            }

        public IActionResult courseDetails(int CrsId, int UserId)
        {
            var student = unit.studentRepo.getByUserId(UserId);
             var stEx = unit.studentExamRepo.getStudentExamByStudentAndCourse(CrsId, student.StudentId);

            return View(stEx);
        }

        [HttpGet]
        public IActionResult CourseList(int instructorId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var insId = int.Parse(userClaim.Value);
            var ins = unit.instructorRepo.getByUserId(insId);

            var courses = unit.courseRepo.InstructorCourses(ins.InsId);
            return View(courses);
        }
        [HttpGet]
        public IActionResult CourseStudentDetails(int crsId)
        {
            var students = unit.studentCourseRepo.GetStudentCourses(crsId);
            return View(students);
        }

        [HttpGet]
        public IActionResult ExamPage()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var insId = int.Parse(userClaim.Value);
            var ins = unit.instructorRepo.getByUserId(insId);

            var exams = unit.examRepo.ExamByInstructorId(ins.InsId);
            return View(exams);
        }

        [HttpGet]
        public IActionResult GenerateExamForTrack()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userClaim.Value);
            var instructor = unit.instructorRepo.getByUserId(userId);

            var courses = unit.courseRepo.InstructorCourses(instructor.InsId);

            var viewModel = new ExamCreationViewModel
            {
                InstructorCourses = courses
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult GenerateExamForTrack(ExamCreationViewModel model)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userClaim.Value);
            var instructor = unit.instructorRepo.getByUserId(userId);

            model.InstructorCourses = unit.courseRepo.InstructorCourses(instructor.InsId);

            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time");
                return View(model);
            }

            try
            {
                unit.examRepo.GenerateAndAssignExam(
                    model.SelectedCourseId,
                    model.MCQCount,
                    model.TFCount,
                    instructor.InsId,
                    model.StartTime,
                    model.EndTime);

                unit.save();
                TempData["SuccessMessage"] = "Exam generated successfully!";

                return RedirectToAction("ExamPage");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while generating the exam: " + ex.Message);
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult GenerateExamForStudent(int? id)
        {
            var UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ins = unit.instructorRepo.getByUserId(UserId);
            if (ins == null)
            {
                return NotFound("Instructor not found");
            }
            //var st = unit.studentRepo.getByUserId(UserId);
            var courses = unit.courseRepo.GetCoursesByInstructor(ins.InsId);
            List<Student> studs = new List<Student>();
            if (id.HasValue)
            {
                studs = unit.studentRepo.GetStudentByInstructorAndCourse(ins.InsId, id.Value);
            }

            var mappedStudents = studs.Select(s => new StudentViewModelForInstructor
            {
                UserId = (int)s.UserId,
                username = s.User?.Username,
                email = s.User?.Email,
                image = s.User?.Img,
                trackname = s.Track?.TrackName
            }).ToList();

            var st = new StudentFilterViewModelForInstructor
            {
                selectedCourse = id,
                students = mappedStudents,
                courses = courses
            };
            return View("GenerateExamForStudent", st);
        }

        [HttpGet]
        public IActionResult AddExamToStudent(int userId, int courseId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var loggedInUserId = int.Parse(userClaim.Value);
            var instructor = unit.instructorRepo.getByUserId(loggedInUserId); 
            var instructorId = instructor.InsId;

            var student = unit.studentRepo.getByUserId(userId);
            var studentId = student.StudentId;

            var model = new AddExamToStudentViewModel
            {
                SelectedCourseId = courseId,
                InstructorId = instructorId,
                StudentId = studentId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddExamToStudent(AddExamToStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                unit.examRepo.GenerateAndAssignExamForStudent(
                    model.SelectedCourseId,
                    model.MCQCount,
                    model.TFCount,
                    model.InstructorId,
                    model.StudentId,
                    model.StartTime,
                    model.EndTime
                );
                unit.save();
                return RedirectToAction("ExamPage");  
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while generating the exam: " + ex.Message);
                return View(model);
            }
        }

    }

}