using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ExSystemProject.Controllers
{
    public class CourseController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Course
        public IActionResult Index()
        {
            // Pass null to get both active and inactive courses
            var courses = _unitOfWork.courseRepo.GetAllCourses(null);
            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);
            return View(courseDTOs);
        }

        // GET: Course/Details/5
        public IActionResult Details(int id)
        {
            // Using the enhanced repository method to get course by id
            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);

            // Get additional course information
            var topics = _unitOfWork.courseRepo.GetCourseTopics(id);
            var exams = _unitOfWork.courseRepo.GetExamsByCourseId(id);

            ViewBag.Topics = topics;
            ViewBag.Exams = exams;

            return View(courseDTO);
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            // Get instructors for dropdown
            var instructors = _unitOfWork.instructorRepo.getAll();
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseDTO courseDTO)
        {
            if (ModelState.IsValid)
            {
                var course = _mapper.Map<Course>(courseDTO);

                // Using the enhanced repository method to create a course
                _unitOfWork.courseRepo.CreateCourse(course);

                TempData["Success"] = true;
                TempData["Message"] = $"Course '{courseDTO.CrsName}' has been created successfully.";

                return RedirectToAction(nameof(Index));
            }

            var instructors = _unitOfWork.instructorRepo.getAll();
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username");
            return View(courseDTO);
        }


        // GET: Course/Edit/5
        public IActionResult Edit(int id)
        {
            // Using the enhanced repository method to get course by id
            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);

            var instructors = _unitOfWork.instructorRepo.getAll();
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", course.InsId);

            return View(courseDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CourseDTO courseDTO)
        {
            if (id != courseDTO.CrsId)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Get current course to compare isactive state
                var currentCourse = _unitOfWork.courseRepo.GetCourseById(id);
                var wasActive = currentCourse?.Isactive ?? true;

                // The issue: courseDTO.Isactive might be null when the checkbox is unchecked
                // Fix: Ensure isactive always has an explicit value, never null
                if (courseDTO.Isactive == null)
                {
                    // In web forms, unchecked checkboxes don't send any value
                    // This is likely why isactive is null when deactivating
                    courseDTO.Isactive = false;
                }

                System.Diagnostics.Debug.WriteLine($"Incoming IsActive (after fix): {courseDTO.Isactive}");
                System.Diagnostics.Debug.WriteLine($"Previous IsActive: {wasActive}");

                var course = _mapper.Map<Course>(courseDTO);

                // Double-check that isactive has an explicit value
                System.Diagnostics.Debug.WriteLine($"Mapped Course IsActive: {course.Isactive}");

                // Using the enhanced repository method to update a course
                _unitOfWork.courseRepo.UpdateCourse(course);

                TempData["Success"] = true;

                // Custom message based on active status change
                if (wasActive == true && courseDTO.Isactive == false)
                {
                    TempData["Message"] = $"Course '{courseDTO.CrsName}' has been deactivated successfully.";
                }
                else if (wasActive == false && courseDTO.Isactive == true)
                {
                    TempData["Message"] = $"Course '{courseDTO.CrsName}' has been activated successfully.";
                }
                else
                {
                    TempData["Message"] = $"Course '{courseDTO.CrsName}' has been updated successfully.";
                }

                return RedirectToAction(nameof(Index));
            }

            var instructors = _unitOfWork.instructorRepo.getAll();
            ViewBag.Instructors = new SelectList(instructors, "InsId", "User.Username", courseDTO.InsId);

            return View(courseDTO);
        }



        // GET: Course/Delete/5
        public IActionResult Delete(int id)
        {
            // Using the enhanced repository method to get course by id
            var course = _unitOfWork.courseRepo.GetCourseById(id);
            if (course == null)
                return NotFound();

            var courseDTO = _mapper.Map<CourseDTO>(course);
            return View(courseDTO);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Using the enhanced repository method to delete a course
            _unitOfWork.courseRepo.DeleteCourse(id);
            return RedirectToAction(nameof(Index));
        }
       
      

    }
}
