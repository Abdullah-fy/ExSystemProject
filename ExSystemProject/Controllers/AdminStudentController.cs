using AutoMapper;
using ExSystemProject.DTOs.Student;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Controllers
{
    public class AdminStudentController : Controller
    {
        private  UnitOfWork _unitOfWork;
        private  IMapper _mapper;
        private  ILogger<AdminStudentController> _logger;

        public AdminStudentController(UnitOfWork unitOfWork, IMapper mapper, ILogger<AdminStudentController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(bool showInactive = false)
        {
            try
            {
                var students = _unitOfWork.studentRepo.getAll()
                    .AsQueryable() // Convert to IQueryable to use Include
                    .Include(s => s.User)
                    .Include(s => s.Track)
                    .Where(s => showInactive || s.Isactive == false)
                    .ToList();

                var studentDTOs = _mapper.Map<List<ShowStudentDTO>>(students);
                ViewBag.ShowInactive = showInactive;
                ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();

                return View(studentDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting students.");
                return View("Error");
            }
        }

        public IActionResult Details(int id)
        {
            try
            {
                var student = _unitOfWork.studentRepo.getAll()
                    .AsQueryable() // Convert to IQueryable to use Include
                    .Include(s => s.User)
                    .Include(s => s.Track)
                    .FirstOrDefault(s => s.StudentId == id);

                if (student == null)
                {
                    return NotFound();
                }

                var studentDTO = _mapper.Map<ShowStudentDTO>(student);
                return View(studentDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id}");
                return StatusCode(500, "An error occurred while retrieving the student");
            }
        }

        public IActionResult Create()
        {
            ViewBag.Tracks = GetTrackSelectList();

            //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateStudentDTO createStudentDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _mapper.Map<User>(createStudentDTO);
                    _unitOfWork.userRepo.add(user);
                    _unitOfWork.save();

                    var student = _mapper.Map<Student>(createStudentDTO);
                    student.UserId = user.UserId;
                    _unitOfWork.studentRepo.add(student);
                    _unitOfWork.save();

                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                ModelState.AddModelError("", "An error occurred while creating the student");
            }

            //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();
            ViewBag.Tracks = GetTrackSelectList();
            return View(createStudentDTO);
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var student = _unitOfWork.studentRepo.getAll()
                    .AsQueryable() // Convert to IQueryable to use Include
                    .Include(s => s.User)
                    .Include(s => s.Track)
                    .FirstOrDefault(s => s.StudentId == id);

                if (student == null)
                {
                    return NotFound();
                }

                var updateStudentDTO = _mapper.Map<UpdateStudentDTO>(student);
                updateStudentDTO.IsActive = (bool)!student.Isactive;

                ViewBag.Tracks = GetTrackSelectList(student.TrackId);
                return View(updateStudentDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} for edit");
                return StatusCode(500, "An error occurred while retrieving the student for editing");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UpdateStudentDTO updateStudentDTO)
        {
            if (id != updateStudentDTO.StudentId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var student = _unitOfWork.studentRepo.getAll()
                        .AsQueryable()
                        .Include(s => s.User)
                        .FirstOrDefault(s => s.StudentId == id);

                    //if (student == null)
                    //{
                    //    return NotFound();
                    //}

                    //_mapper.Map(updateStudentDTO, student.User);
                    //_mapper.Map(updateStudentDTO, student);

                    //_unitOfWork.userRepo.update(student.User);
                    //_unitOfWork.studentRepo.update(student);
                    //_unitOfWork.save();

                    //return RedirectToAction(nameof(Index));
                    if (student != null)
                    {
                        // Toggle the status
                        student.Isactive = !student.Isactive;
                        student.User.Isactive = student.Isactive;

                        _unitOfWork.studentRepo.update(student);
                        _unitOfWork.userRepo.update(student.User);
                        _unitOfWork.save();
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating student with ID {id}");
                ModelState.AddModelError("", "An error occurred while updating the student");
            }

            //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();
            ViewBag.Tracks = GetTrackSelectList(updateStudentDTO.TrackId);
            return View(updateStudentDTO);
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var student = _unitOfWork.studentRepo.getAll()
                    .AsQueryable() // Convert to IQueryable to use Include
                    .Include(s => s.User)
                    .Include(s => s.Track)
                    .FirstOrDefault(s => s.StudentId == id);

                if (student == null)
                {
                    return NotFound();
                }

                var studentDTO = _mapper.Map<ShowStudentDTO>(student);
                return View(studentDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} for deletion");
                return StatusCode(500, "An error occurred while retrieving the student for deletion");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var student = _unitOfWork.studentRepo.getAll()
                    .AsQueryable()
                    .Include(s => s.User)
                    .FirstOrDefault(s => s.StudentId == id);

                if (student != null)
                {
                    //student.Isactive = true;
                    //student.User.Isactive = false;
                    student.Isactive = !(student.Isactive ?? true);
                    student.User.Isactive = student.Isactive;

                    _unitOfWork.studentRepo.update(student);
                    _unitOfWork.userRepo.update(student.User);
                    _unitOfWork.save();
                    TempData["SuccessMessage"] = $"Student status changed to {(student.Isactive.Value ? "Active" : "Inactive")}";

                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting student with ID {id}");
                ModelState.AddModelError("", "An error occurred while deleting the student");
                return View("Delete", _mapper.Map<ShowStudentDTO>(_unitOfWork.studentRepo.getById(id)));
            }
        }
        private List<SelectListItem> GetTrackSelectList(int? selectedTrackId = null)
        {
            var tracks = _unitOfWork.trackRepo.getAll().ToList();
            return tracks.Select(t => new SelectListItem
            {
                Value = t.TrackId.ToString(),
                Text = t.TrackName,
                Selected = t.TrackId == selectedTrackId
            }).ToList();

        }
    }
}