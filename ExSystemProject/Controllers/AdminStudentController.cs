using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class AdminStudentController : SuperAdminBaseController
    {
        private readonly IMapper _mapper;

        public AdminStudentController(UnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        // GET: AdminStudent
        public IActionResult Index(int? branchId = null, int? trackId = null, bool? activeOnly = null)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            List<Student> students;

            if (branchId.HasValue && trackId.HasValue)
            {
                Track track = _unitOfWork.trackRepo.getById(trackId.Value);
                Branch branch = _unitOfWork.branchRepo.getById(branchId.Value);
                // Get students for a specific branch and track
                students = _unitOfWork.studentRepo.GetStudentsByBranchAndTrackName(branch.BranchName, track.TrackName, activeOnly);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = _unitOfWork.branchRepo.getById(branchId.Value)?.BranchName;
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = _unitOfWork.trackRepo.getById(trackId.Value)?.TrackName;
            }
            else if (branchId.HasValue)
            {
                // Get students for a specific branch
                students = _unitOfWork.studentRepo.GetStudentsByBranchId(branchId.Value, activeOnly);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = _unitOfWork.branchRepo.getById(branchId.Value)?.BranchName;
            }
            else if (trackId.HasValue)
            {
                // Get students for a specific track
                students = _unitOfWork.studentRepo.GetStudentsByTrackId(trackId.Value, activeOnly);
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = _unitOfWork.trackRepo.getById(trackId.Value)?.TrackName;
            }
            else
            {
                // Get all students
                students = _unitOfWork.studentRepo.GetAllStudents(activeOnly);
            }

            var branches = _unitOfWork.branchRepo.getAll();
            var tracks = _unitOfWork.trackRepo.GetDistictTracks();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", branchId);
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", trackId);
            ViewBag.ActiveOnly = activeOnly;

            var studentDTOs = _mapper.Map<List<StudentDTO>>(students);
            return View(studentDTOs);
        }

        // GET: AdminStudent/Details/5
        public IActionResult Details(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var student = _unitOfWork.studentRepo.GetStudentById(id);
            if (student == null)
                return NotFound();

            // Explicitly include related data
            student = _unitOfWork.context.Students
                .Include(s => s.User)
                .Include(s => s.Track)
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Crs)
                .Include(s => s.StudentExams)
                    .ThenInclude(se => se.Exam)
                .FirstOrDefault(s => s.StudentId == id);

            var studentDTO = _mapper.Map<StudentDTO>(student);

            return View(studentDTO);
        }

        // GET: AdminStudent/Create
        public IActionResult Create()
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            // Get all branches
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

            // When a branch is selected, we'll use JavaScript to populate tracks for that branch
            var tracks = _unitOfWork.trackRepo.getAll();
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName");

            return View();
        }


       
        // POST: AdminStudent/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentDTO studentDTO, string Password)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            try
            {
                // Log received data for debugging
                System.Diagnostics.Debug.WriteLine($"Creating student: {studentDTO.Username}, Email: {studentDTO.Email}, TrackId: {studentDTO.TrackId}, TrackName: {studentDTO.TrackName}, BranchName: {studentDTO.BranchName}");

                // Check for required fields
                if (string.IsNullOrEmpty(studentDTO.Username) ||
                    string.IsNullOrEmpty(studentDTO.Email) ||
                    string.IsNullOrEmpty(studentDTO.Gender))
                {
                    ModelState.AddModelError("", "Please fill in all required fields");
                }

                // Check for required password
                if (string.IsNullOrEmpty(Password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                }

                if (ModelState.IsValid)
                {
                    // Use the CreateStudentWithStoredProcedure method with admin-provided password
                    _unitOfWork.studentRepo.CreateStudentWithStoredProcedure(
                        studentDTO.Username,
                        studentDTO.Email,
                        studentDTO.Gender,
                        Password, // Use the password entered by the admin
                        studentDTO.TrackId);

                    TempData["SuccessMessage"] = "Student created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating student: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception in Create: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            // If we get here, there was an error - reload the form with the same data
            var allBranches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(allBranches, "BranchId", "BranchName");

            // If TrackId is provided, populate tracks for that branch
            if (studentDTO.TrackId.HasValue && studentDTO.BranchId.HasValue)
            {
                var tracks = _unitOfWork.branchRepo.GetTracksByBranchId(studentDTO.BranchId.Value);
                ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", studentDTO.TrackId);
            }

            return View(studentDTO);
        }



        // GET: AdminStudent/Edit/5
        public IActionResult Edit(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var student = _unitOfWork.studentRepo.GetStudentByIdWithBranch(id);
            if (student == null)
                return NotFound();

            var studentDTO = _mapper.Map<StudentDTO>(student);

            // Get all branches for dropdown
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = branches;

            // Get tracks for the student's branch (if any)
            List<Track> tracks;
            if (student.Track?.BranchId > 0)
            {
                tracks = _unitOfWork.branchRepo.GetTracksByBranchId(student.Track.BranchId.Value);
            }
            else
            {
                tracks = _unitOfWork.trackRepo.getAll();
            }
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", student.TrackId);

            return View(studentDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StudentDTO studentDTO, int? BranchId, string IsactiveHidden)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            System.Diagnostics.Debug.WriteLine($"Edit POST called with: ID={id}, Username={studentDTO.Username}, " +
                $"Email={studentDTO.Email}, Gender={studentDTO.Gender}, TrackId={studentDTO.TrackId}, " +
                $"BranchId={BranchId}, IsActiveHidden={IsactiveHidden}");

            if (id != studentDTO.StudentId)
                return NotFound();

            // Explicitly set the active status based on the hidden field value
            studentDTO.Isactive = IsactiveHidden?.ToLower() == "true";
            System.Diagnostics.Debug.WriteLine($"Setting Isactive to: {studentDTO.Isactive}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Verify that the track belongs to the selected branch if a branch was selected
                    if (BranchId.HasValue && studentDTO.TrackId.HasValue)
                    {
                        var track = _unitOfWork.trackRepo.getById(studentDTO.TrackId.Value);
                        if (track == null)
                        {
                            ModelState.AddModelError("TrackId", "Selected track not found");
                            PrepareEditViewBags(BranchId, studentDTO);
                            return View(studentDTO);
                        }

                        if (track.BranchId != BranchId)
                        {
                            ModelState.AddModelError("TrackId", "Selected track doesn't belong to the chosen branch");
                            PrepareEditViewBags(BranchId, studentDTO);
                            return View(studentDTO);
                        }

                        System.Diagnostics.Debug.WriteLine($"Track validated. Track belongs to branch: {track.BranchId}");
                    }

                    // Ensure isactive is properly handled (read from the explicit value we set above)
                    bool isActive = studentDTO.Isactive ?? true;
                    System.Diagnostics.Debug.WriteLine($"IsActive value being used: {isActive}");

                    // Update student using stored procedure
                    _unitOfWork.studentRepo.UpdateStudent(
                        studentDTO.StudentId,
                        studentDTO.Username,
                        studentDTO.Email,
                        studentDTO.Gender,
                        studentDTO.TrackId,
                        isActive
                    );

                    System.Diagnostics.Debug.WriteLine("Student updated successfully");
                    TempData["SuccessMessage"] = "Student updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = studentDTO.StudentId });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating student: {ex.Message}");
                    ModelState.AddModelError("", $"Error updating student: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"- {state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            PrepareEditViewBags(BranchId, studentDTO);
            return View(studentDTO);
        }

        private void PrepareEditViewBags(int? branchId, StudentDTO studentDTO)
        {
            var allBranches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = allBranches;

            var allTracks = branchId.HasValue
                ? _unitOfWork.branchRepo.GetTracksByBranchId(branchId.Value)
                : studentDTO.BranchId.HasValue
                    ? _unitOfWork.branchRepo.GetTracksByBranchId(studentDTO.BranchId.Value)
                    : _unitOfWork.trackRepo.getAll();

            ViewBag.Tracks = new SelectList(allTracks, "TrackId", "TrackName", studentDTO.TrackId);
        }

        // GET: AdminStudent/Delete/5
        public IActionResult Delete(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var student = _unitOfWork.studentRepo.GetStudentById(id);
            if (student == null)
                return NotFound();

            var studentDTO = _mapper.Map<StudentDTO>(student);
            return View(studentDTO);
        }

        // POST: AdminStudent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            try
            {
                // Delete student using stored procedure (logical delete)
                _unitOfWork.studentRepo.DeleteStudent(id);

                TempData["SuccessMessage"] = "Student deactivated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deactivating student: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        // GET: AdminStudent/AssignExam
        public IActionResult AssignExam(int id)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            var student = _unitOfWork.studentRepo.GetStudentById(id);
            if (student == null)
                return NotFound();

            var allExams = _unitOfWork.examRepo.GetAllExams();
            var availableExams = allExams.Where(e => e.Isactive == true).ToList();

            ViewBag.Exams = new SelectList(availableExams, "ExamId", "ExamName");

            ViewBag.StudentId = id;
            ViewBag.StudentName = student.User?.Username;

            return View();
        }

        // POST: AdminStudent/AssignExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignExam(int studentId, int examId)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            try
            {
                bool ExamAssigned = _unitOfWork.studentExamRepo.ExamAssigned(studentId, examId);
                if (ExamAssigned)
                {
                    TempData["SwalType"] = "warning";
                    TempData["SwalMessage"] = "Exam already assigned.";
                    return RedirectToAction(nameof(Details), new { id = studentId });
                }
                // Assign exam to student using stored procedure
                _unitOfWork.studentRepo.AssignExamToStudent(examId, studentId);

                TempData["SuccessMessage"] = "Exam assigned successfully!";
                return RedirectToAction(nameof(Details), new { id = studentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error assigning exam: {ex.Message}";

                var allExams = _unitOfWork.examRepo.GetAllExams();
                var availableExams = allExams.Where(e => e.Isactive == true).ToList();

                ViewBag.Exams = new SelectList(availableExams, "ExamId", "ExamName", examId);

                var student = _unitOfWork.studentRepo.GetStudentById(studentId);
                ViewBag.StudentId = studentId;
                ViewBag.StudentName = student.User?.Username;

                return View();
            }
        }

        [HttpGet]
        public JsonResult GetTracksByBranch(int branchId)
        {
            // Get current user ID using the base controller method
            var userId = GetCurrentUserId();

            try
            {
                System.Diagnostics.Debug.WriteLine($"GetTracksByBranch called with branchId: {branchId}");

                var tracks = _unitOfWork.branchRepo.GetTracksByBranchId(branchId)
                    .Where(t => t.IsActive == true) // Only get active tracks
                    .Select(t => new { trackId = t.TrackId, trackName = t.TrackName })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Found {tracks.Count} tracks for branch {branchId}");
                return Json(tracks);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTracksByBranch: {ex.Message}");
                return Json(new List<object>());
            }
        }
    }
}
