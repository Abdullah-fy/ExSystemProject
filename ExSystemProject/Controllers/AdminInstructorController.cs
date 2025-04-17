using AutoMapper;
using ExSystemProject.DTOs.Instructor;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Controllers
{
    public class AdminInstructorController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminInstructorController> _logger;

        public AdminInstructorController(UnitOfWork unitOfWork, IMapper mapper, ILogger<AdminInstructorController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(bool showInactive = false)
        {
            try
            {
                var instructors = _unitOfWork.instructorRepo.getAll()
                     .AsQueryable() // Convert to IQueryable to use Include
                    .Include(i => i.User)
                    .Include(i => i.Track)
                    .Where(i => showInactive || i.Isactive == true)
                    .ToList();

                var instructorDTOs = _mapper.Map<List<InstructorDTO>>(instructors);
                ViewBag.ShowInactive = showInactive;
                ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();

                return View(instructorDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving instructors");
                return View("Error");
            }
        }

        public IActionResult Details(int id)
        {
            try
            {
                var instructor = _unitOfWork.instructorRepo.getAll()
                     .AsQueryable() // Convert to IQueryable to use Include
                    .Include(i => i.User)
                    .Include(i => i.Track)
                    .FirstOrDefault(i => i.InsId == id);

                if (instructor == null)
                {
                    return NotFound();
                }

                var instructorDTO = _mapper.Map<InstructorDTO>(instructor);
                return View(instructorDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving instructor with ID {id}");
                return StatusCode(500, "An error occurred while retrieving the instructor");
            }
        }

        public IActionResult Create()
        {
            //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();
            ViewBag.Tracks = GetTrackSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateInstructorDTO createInstructorDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _mapper.Map<User>(createInstructorDTO);
                    _unitOfWork.userRepo.add(user);
                    _unitOfWork.save();

                    var instructor = _mapper.Map<Instructor>(createInstructorDTO);
                    instructor.UserId = user.UserId;
                    _unitOfWork.instructorRepo.add(instructor);
                    _unitOfWork.save();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating instructor");
                ModelState.AddModelError("", "An error occurred while creating the instructor");
            }

            //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();
            ViewBag.Tracks = GetTrackSelectList();

            return View(createInstructorDTO);
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var instructor = _unitOfWork.instructorRepo.getAll()
                     .AsQueryable() // Convert to IQueryable to use Include
                    .Include(i => i.User)
                    .Include(i => i.Track)
                    .FirstOrDefault(i => i.InsId == id);

                if (instructor == null)
                {
                    return NotFound();
                }
                // Ensure ViewBag.Tracks is never null
                ViewBag.Tracks = _unitOfWork.trackRepo.getAll()
                    .Select(t => new SelectListItem
                    {
                        Value = t.TrackId.ToString(),
                        Text = t.TrackName,
                        Selected = t.TrackId == instructor.TrackId
                    })
                    .ToList() ?? new List<SelectListItem>();


                var updateInstructorDTO = _mapper.Map<UpdateInstructorDTO>(instructor);
                //updateInstructorDTO.IsActive = instructor.Isactive ?? false;

                //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();

                return View(updateInstructorDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving instructor with ID {id} for edit");
                return StatusCode(500, "An error occurred while retrieving the instructor for editing");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UpdateInstructorDTO updateInstructorDTO)
        {
            if (id != updateInstructorDTO.InsId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var instructor = _unitOfWork.instructorRepo.getAll()
                         .AsQueryable() // Convert to IQueryable to use Include
                        .Include(i => i.User)
                        .FirstOrDefault(i => i.InsId == id);

                    if (instructor == null)
                    {
                        return NotFound();
                    }

                    _mapper.Map(updateInstructorDTO, instructor.User);
                    _mapper.Map(updateInstructorDTO, instructor);

                    _unitOfWork.userRepo.update(instructor.User);
                    _unitOfWork.instructorRepo.update(instructor);
                    _unitOfWork.save();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating instructor with ID {id}");
                ModelState.AddModelError("", "An error occurred while updating the instructor");
            }

            //ViewBag.Tracks = _unitOfWork.trackRepo.getAll().ToList();
            ViewBag.Tracks = GetTrackSelectList(updateInstructorDTO.TrackId);

            return View(updateInstructorDTO);
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var instructor = _unitOfWork.instructorRepo.getAll()
                     .AsQueryable() // Convert to IQueryable to use Include
                    .Include(i => i.User)
                    .Include(i => i.Track)
                    .FirstOrDefault(i => i.InsId == id);

                if (instructor == null)
                {
                    return NotFound();
                }

                var instructorDTO = _mapper.Map<InstructorDTO>(instructor);
                return View(instructorDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving instructor with ID {id} for deletion");
                return StatusCode(500, "An error occurred while retrieving the instructor for deletion");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var instructor = _unitOfWork.instructorRepo.getAll()
                     .AsQueryable() // Convert to IQueryable to use Include
                    .Include(i => i.User)
                    .FirstOrDefault(i => i.InsId == id);

                if (instructor != null)
                {
                    instructor.Isactive = !instructor.Isactive;
                    instructor.User.Isactive = instructor.Isactive;
                    _unitOfWork.instructorRepo.update(instructor);
                    _unitOfWork.userRepo.update(instructor.User);
                    _unitOfWork.save();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting instructor with ID {id}");
                ModelState.AddModelError("", "An error occurred while deleting the instructor");
                return View("Delete", _mapper.Map<InstructorDTO>(_unitOfWork.instructorRepo.getById(id)));
            }
        }

        private List<SelectListItem> GetTrackSelectList(int? selectedTrackId = null)
        {
            return _unitOfWork.trackRepo.getAll()
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),  // Must use "Value"
                    Text = t.TrackName,            // Must use "Text"
                    Selected = t.TrackId == selectedTrackId
                }).ToList();
        }

    }
}