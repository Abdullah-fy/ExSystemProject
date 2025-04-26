using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using ExSystemProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

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

            // Get students from this branch only
            var students = _unitOfWork.studentRepo.GetStudentsByBranchId(CurrentBranchId, active);
            return View(students);
        }

        // GET: BranchManagerStudent/Details/5
        public IActionResult Details(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            // Check if student exists and belongs to this branch
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
            // Only get tracks from the current branch
            var tracks = _unitOfWork.trackRepo.getAll()
                .Where(t => t.BranchId == CurrentBranchId && t.IsActive == true)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName
                })
                .ToList();

            var model = new StudentViewModel
            {
                tracks = tracks
            };

            ViewData["Title"] = "Add New Student";
            return View(model);
        }

        // POST: BranchManagerStudent/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verify the selected track belongs to this branch
                    if (!string.IsNullOrEmpty(model.TrackId))
                    {
                        int trackId = int.Parse(model.TrackId);
                        var track = _unitOfWork.trackRepo.getById(trackId);

                        if (track == null || track.BranchId != CurrentBranchId)
                        {
                            ModelState.AddModelError("TrackId", "Invalid track selection.");
                            return View(model);
                        }
                    }

                    _unitOfWork.studentRepo.AddNewStudent(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating student: {ex.Message}");
                }
            }

            // Repopulate tracks if there was an error
            model.tracks = _unitOfWork.trackRepo.getAll()
                .Where(t => t.BranchId == CurrentBranchId && t.IsActive == true)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName
                })
                .ToList();

            return View(model);
        }

        // GET: BranchManagerStudent/Edit/5
        public IActionResult Edit(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            // Check if student exists and belongs to this branch
            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Get tracks from the current branch
            var tracks = _unitOfWork.trackRepo.getAll()
                .Where(t => t.BranchId == CurrentBranchId && t.IsActive == true)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName,
                    Selected = t.TrackId == student.TrackId
                })
                .ToList();

            var viewModel = new StudentEditViewModel
            {
                StudentId = student.StudentId,
                Username = student.User?.Username,
                Email = student.User?.Email,
                Gender = student.User?.Gender,
                TrackId = student.TrackId,
                IsActive = student.Isactive ?? true,
                Tracks = tracks
            };

            ViewData["Title"] = "Edit Student";
            return View(viewModel);
        }

        // POST: BranchManagerStudent/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StudentEditViewModel model)
        {
            if (id != model.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verify the track belongs to this branch
                    if (model.TrackId.HasValue)
                    {
                        var track = _unitOfWork.trackRepo.getById(model.TrackId.Value);
                        if (track == null || track.BranchId != CurrentBranchId)
                        {
                            ModelState.AddModelError("TrackId", "Invalid track selection");
                            return View(model);
                        }
                    }

                    // Update the student
                    _unitOfWork.studentRepo.UpdateStudent(
                        model.StudentId,
                        model.Username,
                        model.Email,
                        model.Gender,
                        model.TrackId,
                        model.IsActive
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating student: {ex.Message}");
                }
            }

            // Repopulate tracks dropdown if there was an error
            model.Tracks = _unitOfWork.trackRepo.getAll()
                .Where(t => t.BranchId == CurrentBranchId && t.IsActive == true)
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName,
                    Selected = t.TrackId == model.TrackId
                })
                .ToList();

            return View(model);
        }

        // GET: BranchManagerStudent/Delete/5
        public IActionResult Delete(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            // Check if student exists and belongs to this branch
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

            // Check if student exists and belongs to this branch
            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            // Soft delete - calls stored procedure that sets isactive = false
            _unitOfWork.studentRepo.DeleteStudent(id);

            return RedirectToAction(nameof(Index));
        }

        // GET: BranchManagerStudent/Assignments/5
        public IActionResult Assignments(int id)
        {
            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);

            // Check if student exists and belongs to this branch
            if (student == null || student.Track?.BranchId != CurrentBranchId)
            {
                return NotFound();
            }

            ViewData["Title"] = "Student Course Assignments";
            return View(student);
        }
        // GET: BranchManagerStudent/GetAvailableCourses
        [HttpGet]
        public IActionResult GetAvailableCourses(int studentId, int branchId)
        {
            // Get courses from the branch that the student isn't already enrolled in
            var courses = _unitOfWork.courseRepo.GetAllCourses(true)
                .Where(c => c.Ins?.Track?.BranchId == branchId)
                .Where(c => !c.StudentCourses.Any(sc => sc.StudentId == studentId && sc.Isactive == true))
                .Select(c => new {
                    CrsId = c.CrsId,
                    CrsName = c.CrsName,
                    InstructorName = c.Ins?.User?.Username
                })
                .ToList();

            return Json(courses);
        }

        // POST: BranchManagerStudent/EnrollInCourse
        [HttpPost]
        public IActionResult EnrollInCourse(int studentId, int courseId)
        {
            try
            {
                // Validate the student belongs to this branch
                var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(studentId);
                if (student == null || student.Track?.BranchId != CurrentBranchId)
                {
                    return NotFound("Student not found or doesn't belong to your branch");
                }

                // Validate the course belongs to this branch
                var course = _unitOfWork.courseRepo.GetCourseById(courseId);
                if (course == null || course.Ins?.Track?.BranchId != CurrentBranchId)
                {
                    return NotFound("Course not found or doesn't belong to your branch");
                }

                // Check if already enrolled
                if (_unitOfWork.studentCourseRepo.IsStudentEnrolled(studentId, courseId))
                {
                    TempData["Error"] = "Student is already enrolled in this course";
                    return RedirectToAction(nameof(Assignments), new { id = studentId });
                }

                // Enroll the student
                _unitOfWork.studentCourseRepo.EnrollStudent(studentId, courseId);
                TempData["Success"] = "Student successfully enrolled in the course";

                return RedirectToAction(nameof(Assignments), new { id = studentId });
            }
            catch (Exception ex)
            {
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
                // Validate the student belongs to this branch
                var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(studentId);
                if (student == null || student.Track?.BranchId != CurrentBranchId)
                {
                    return NotFound("Student not found or doesn't belong to your branch");
                }

                // Unenroll the student (set enrollment to inactive)
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


    }
}
