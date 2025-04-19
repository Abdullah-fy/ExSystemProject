using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Controllers
{
    public class AdminStudentController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminStudentController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: AdminStudent
        public IActionResult Index(int? branchId = null, int? trackId = null, bool? activeOnly = true)
        {
            List<Student> students;
            
            if (branchId.HasValue)
            {
                // Get students for a specific branch using stored procedure
                students = _unitOfWork.studentRepo.GetStudentsByBranchId(branchId.Value, activeOnly);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = _unitOfWork.branchRepo.getById(branchId.Value)?.BranchName;
            }
            else if (trackId.HasValue)
            {
                // Get students for a specific track using stored procedure
                students = _unitOfWork.studentRepo.GetStudentsByTrackId(trackId.Value, activeOnly);
                ViewBag.TrackId = trackId;
                ViewBag.TrackName = _unitOfWork.trackRepo.getById(trackId.Value)?.TrackName;
            }
            else
            {
                // Get all students using stored procedure
                students = _unitOfWork.studentRepo.GetAllStudents(activeOnly);
            }

            var branches = _unitOfWork.branchRepo.getAll();
            var tracks = _unitOfWork.trackRepo.getAll();
            
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", branchId);
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", trackId);
            ViewBag.ActiveOnly = activeOnly;

            var studentDTOs = _mapper.Map<List<StudentDTO>>(students);
            return View(studentDTOs);
        }

        // GET: AdminStudent/Details/5
        public IActionResult Details(int id)
        {
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
            // Get all branches
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");

            // When a branch is selected, we'll use JavaScript to populate tracks for that branch
            var tracks = _unitOfWork.trackRepo.getAll();
            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentDTO studentDTO, int BranchId)
        {
            // Log input values for debugging
            System.Diagnostics.Debug.WriteLine($"Create POST called with: Username={studentDTO.Username}, Email={studentDTO.Email}, Gender={studentDTO.Gender}, TrackId={studentDTO.TrackId}, BranchId={BranchId}");

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if username or email already exists
                    if (_unitOfWork.userRepo.getActiveByName(studentDTO.Username) != null)
                    {
                        ModelState.AddModelError("Username", "Username already exists");
                        PopulateFormDropdowns(BranchId, studentDTO.TrackId);
                        return View(studentDTO);
                    }

                    if (_unitOfWork.userRepo.GetByEmail(studentDTO.Email) != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        PopulateFormDropdowns(BranchId, studentDTO.TrackId);
                        return View(studentDTO);
                    }

                    // Verify that the track belongs to the selected branch
                    if (studentDTO.TrackId.HasValue)
                    {
                        var track = _unitOfWork.trackRepo.getById(studentDTO.TrackId.Value);
                        if (track == null)
                        {
                            ModelState.AddModelError("TrackId", "Selected track not found");
                            PopulateFormDropdowns(BranchId, null);
                            return View(studentDTO);
                        }

                        if (track.BranchId != BranchId)
                        {
                            ModelState.AddModelError("TrackId", "Selected track doesn't belong to the chosen branch");
                            PopulateFormDropdowns(BranchId, null);
                            return View(studentDTO);
                        }

                        // Store the track's branch ID
                        studentDTO.BranchId = track.BranchId;
                        System.Diagnostics.Debug.WriteLine($"Track validated successfully. Track belongs to branch ID: {track.BranchId}");
                    }
                    else
                    {
                        ModelState.AddModelError("TrackId", "Please select a track");
                        PopulateFormDropdowns(BranchId, null);
                        return View(studentDTO);
                    }

                    // Create a new student using stored procedure
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Calling CreateStudentWithStoredProcedure with values: " +
                            $"Username={studentDTO.Username}, Email={studentDTO.Email}, Gender={studentDTO.Gender}, TrackId={studentDTO.TrackId}");

                        _unitOfWork.studentRepo.CreateStudentWithStoredProcedure(
                            studentDTO.Username,
                            studentDTO.Email,
                            studentDTO.Gender,
                            "defaultPassword123",  // Default password
                            studentDTO.TrackId
                        );

                        System.Diagnostics.Debug.WriteLine("CreateStudentWithStoredProcedure completed successfully");

                        TempData["SuccessMessage"] = "Student created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in stored procedure: {ex.Message}");
                        ModelState.AddModelError("", $"Database error: {ex.Message}");
                        PopulateFormDropdowns(BranchId, studentDTO.TrackId);
                        return View(studentDTO);
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in Create action: {ex.Message}");
                ModelState.AddModelError("", $"Error creating student: {ex.Message}");
            }

            // If we got this far, something failed, redisplay form
            System.Diagnostics.Debug.WriteLine("Repopulating dropdowns for failed submission");
            PopulateFormDropdowns(BranchId, studentDTO.TrackId);
            return View(studentDTO);
        }




        private void PopulateFormDropdowns(int? branchId = null, int? trackId = null)
        {
            var branches = _unitOfWork.branchRepo.getAll();
            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", branchId);

            List<Track> tracks = new List<Track>();
            if (branchId.HasValue)
            {
                tracks = _unitOfWork.branchRepo.GetTracksByBranchId(branchId.Value);
            }
            else
            {
                tracks = _unitOfWork.trackRepo.getAll();
            }

            ViewBag.Tracks = new SelectList(tracks, "TrackId", "TrackName", trackId);
        }




        // GET: AdminStudent/Edit/5
        public IActionResult Edit(int id)
        {
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
            var student = _unitOfWork.studentRepo.GetStudentById(id);
            if (student == null)
                return NotFound();

            var studentDTO = _mapper.Map<StudentDTO>(student);
            return View(studentDTO);
        }

        // POST: AdminStudent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)  // Changed method name to DeleteConfirmed
        {
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
            try
            {
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


