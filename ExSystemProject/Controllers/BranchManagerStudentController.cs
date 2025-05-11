using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using ExSystemProject.ViewModels;
using ExSystemProject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Controllers
{
    public class BranchManagerStudentController : BranchManagerBaseController
    {
        public BranchManagerStudentController(UnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        // GET: BranchManagerStudent
        public IActionResult Index(bool? active = true)
        {
            ViewData["Title"] = "Students Management";

            var students = _unitOfWork.studentRepo.GetStudentsByBranchId(CurrentBranchId, active);
            return View(students);
        }

        // GET: BranchManagerStudent/Details/5
        public IActionResult Details(int id)
        {
            var student = _unitOfWork.context.Students
                .Include(s => s.User)
                .Include(s => s.Track)
                    .ThenInclude(t => t.Branch)
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Crs)
                        .ThenInclude(c => c.Ins)
                            .ThenInclude(i => i.User)
                .FirstOrDefault(s => s.StudentId == id);

            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = "Student Details";
            return View(student);
        }


        // GET: BranchManagerStudent/Create
        public IActionResult Create()
        {
            PrepareCreateViewBags();
            return View(new StudentDTO());
        }

        // POST: BranchManagerStudent/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentDTO studentDTO, string Password, string PasswordBackup)
        {
            string finalPassword = !string.IsNullOrEmpty(Password) ? Password : PasswordBackup;

            try
            {
                if (string.IsNullOrEmpty(studentDTO.Username) ||
                    string.IsNullOrEmpty(studentDTO.Email) ||
                    string.IsNullOrEmpty(studentDTO.Gender) ||
                    string.IsNullOrEmpty(finalPassword))
                {
                    ModelState.AddModelError("", "Please fill in all required fields");
                    PrepareCreateViewBags();
                    return View(studentDTO);
                }

                if (finalPassword.Length < 6)
                {
                    ModelState.AddModelError("Password", "Password must be at least 6 characters long");
                    PrepareCreateViewBags();
                    return View(studentDTO);
                }

                _unitOfWork.studentRepo.CreateStudentWithStoredProcedure(
                    studentDTO.Username,
                    studentDTO.Email,
                    studentDTO.Gender,
                    finalPassword,
                    studentDTO.TrackId);

                TempData["SuccessMessage"] = "Student created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating student: {ex.Message}");
                PrepareCreateViewBags();
                return View(studentDTO);
            }
        }

        // GET: BranchManagerStudent/Edit/5
        public IActionResult Edit(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            var studentDTO = new StudentDTO
            {
                StudentId = student.StudentId,
                Username = student.User?.Username,
                Email = student.User?.Email,
                Gender = student.User?.Gender,
                TrackId = student.TrackId,
                Isactive = student.Isactive ?? true 
            };

            PrepareEditViewBags();
            return View(studentDTO);
        }

        // POST: BranchManagerStudent/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StudentDTO studentDTO)
        {
            if (id != studentDTO.StudentId)
            {
                return NotFound();
            }

            try
            {
                if (studentDTO.TrackId.HasValue)
                {
                    var track = _unitOfWork.trackRepo.getById(studentDTO.TrackId.Value);
                    if (track == null || track.BranchId != CurrentBranchId)
                    {
                        ModelState.AddModelError("TrackId", "Invalid track selection");
                        PrepareEditViewBags();
                        return View(studentDTO);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Updating student: ID={studentDTO.StudentId}, " +
                    $"Username={studentDTO.Username}, Email={studentDTO.Email}, " +
                    $"Gender={studentDTO.Gender}, TrackId={studentDTO.TrackId}, " +
                    $"IsActive={studentDTO.Isactive}");

                bool isActive = studentDTO.Isactive ?? true;

                _unitOfWork.studentRepo.UpdateStudent(
                    studentDTO.StudentId,
                    studentDTO.Username,
                    studentDTO.Email,
                    studentDTO.Gender,
                    studentDTO.TrackId,
                    isActive
                );

                TempData["SuccessMessage"] = "Student updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating student: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error updating student: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

            PrepareEditViewBags();
            return View(studentDTO);
        }

        // GET: BranchManagerStudent/Delete/5
        public IActionResult Delete(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = "Delete Student";
            return View(student);
        }

        // POST: BranchManagerStudent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            _unitOfWork.studentRepo.DeleteStudent(id);

            return RedirectToAction(nameof(Index));
        }

        // GET: BranchManagerStudent/Assignments/5
        public IActionResult Assignments(int id)
        {
            try
            {
                var student = _unitOfWork.context.Students
                    .Include(s => s.User)
                    .Include(s => s.Track)
                        .ThenInclude(t => t.Branch)
                    .Include(s => s.StudentCourses)
                        .ThenInclude(sc => sc.Crs)
                            .ThenInclude(c => c.Ins)
                                .ThenInclude(i => i.User)
                    .FirstOrDefault(s => s.StudentId == id);

                if (student == null || student.Track?.BranchId != CurrentBranchId)
                {
                    return NotFound();
                }

                ViewData["Title"] = "Student Course Assignments";
                ViewData["BranchId"] = CurrentBranchId;

                if (TempData["Success"] != null)
                {
                    ViewData["SuccessMessage"] = TempData["Success"];
                }

                if (TempData["Error"] != null)
                {
                    ViewData["ErrorMessage"] = TempData["Error"];
                }

                return View(student);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading student assignments: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }


        // GET: BranchManagerStudent/GetAvailableCourses

        [HttpGet]
        public IActionResult GetAvailableCourses(int studentId)
        {
            try
            {
                var branchCourses = _unitOfWork.courseRepo.GetAllCourses(true)
                    .Where(c => c.Ins?.Track?.BranchId == CurrentBranchId)
                    .ToList();

                var enrolledCourseIds = _unitOfWork.context.StudentCourses
                    .Where(sc => sc.StudentId == studentId && sc.Isactive == true)
                    .Select(sc => sc.CrsId)
                    .ToList();

                var availableCourses = branchCourses
                    .Where(c => !enrolledCourseIds.Contains(c.CrsId))
                    .Select(c => new {
                        CrsId = c.CrsId,
                        CrsName = c.CrsName,
                        InstructorName = c.Ins?.User?.Username ?? "Not Assigned"
                    })
                    .ToList();

                return Json(availableCourses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAvailableCourses: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }




        // POST: BranchManagerStudent/EnrollInCourse
        [HttpPost]
        public IActionResult EnrollInCourse(int studentId, int courseId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EnrollInCourse called: Student ID={studentId}, Course ID={courseId}");

                var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(studentId);
                if (student == null || student.Track?.BranchId != CurrentBranchId)
                {
                    TempData["Error"] = "Student not found or doesn't belong to your branch";
                    return RedirectToAction(nameof(Assignments), new { id = studentId });
                }

                var course = _unitOfWork.courseRepo.GetCourseById(courseId);
                if (course == null)
                {
                    TempData["Error"] = "Course not found";
                    return RedirectToAction(nameof(Assignments), new { id = studentId });
                }

                if (course.Ins?.Track?.BranchId != CurrentBranchId)
                {
                    TempData["Error"] = "Course doesn't belong to your branch";
                    return RedirectToAction(nameof(Assignments), new { id = studentId });
                }

                if (_unitOfWork.studentCourseRepo.IsStudentEnrolled(studentId, courseId))
                {
                    TempData["Error"] = "Student is already enrolled in this course";
                    return RedirectToAction(nameof(Assignments), new { id = studentId });
                }

                _unitOfWork.studentCourseRepo.EnrollStudent(studentId, courseId);

                _unitOfWork.context.Entry(student).State = EntityState.Detached;

                System.Diagnostics.Debug.WriteLine("Enrollment successful");
                TempData["Success"] = "Student successfully enrolled in the course";

                return RedirectToAction(nameof(Assignments), new { id = studentId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enrolling student: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["Error"] = $"Error enrolling student: {ex.Message}";
                return RedirectToAction(nameof(Assignments), new { id = studentId });
            }
        }


        // POST: BranchManagerStudent/UnenrollFromCourse
        [HttpPost]
        public IActionResult UnenrollFromCourse(int studentId, int courseId)
        {
            try
            {
                var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(studentId);
                if (student == null || student.Track?.BranchId != CurrentBranchId)
                {
                    return NotFound("Student not found or doesn't belong to your branch");
                }

                _unitOfWork.studentCourseRepo.UnenrollStudent(studentId, courseId);
                TempData["Success"] = "Student successfully unenrolled from the course";

                return RedirectToAction(nameof(Assignments), new { id = studentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error unenrolling student: {ex.Message}";
                return RedirectToAction(nameof(Assignments), new { id = studentId });
            }
        }

        private void PrepareCreateViewBags()
        {
            var tracks = _unitOfWork.trackRepo.getAll()
                .Where(t => t.BranchId == CurrentBranchId && t.IsActive == true)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName
                })
                .ToList();

            ViewBag.Tracks = new SelectList(tracks, "Value", "Text");
        }

        private void PrepareEditViewBags()
        {
            try
            {
                var tracks = _unitOfWork.trackRepo.getAll()
                    .Where(t => t.BranchId == CurrentBranchId && t.IsActive == true)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TrackId.ToString(),
                        Text = t.TrackName
                    })
                    .ToList();

                ViewBag.Tracks = new SelectList(tracks, "Value", "Text");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PrepareEditViewBags: {ex.Message}");
                ViewBag.Tracks = new SelectList(new List<SelectListItem>(), "Value", "Text");
            }
        }
    }
}
