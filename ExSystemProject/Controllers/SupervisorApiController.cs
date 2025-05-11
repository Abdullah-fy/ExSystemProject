
using AutoMapper;
using ExSystemProject.DTOS;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExSystemProject.Controllers
{
    [Route("api/supervisor")]
    [ApiController]
    [Authorize(Roles = "supervisor")]
    public class SupervisorApiController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupervisorApiController(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        private UserAssignment GetCurrentSupervisor()
        {
            int userId = GetCurrentUserId();
            return _unitOfWork.supervisorRepo.GetSupervisorByUserId(userId);
        }

        [HttpGet("students")]
        public IActionResult GetStudents()
        {
            try
            {
                var supervisor = GetCurrentSupervisor();

                if (supervisor == null)
                    return Unauthorized();

                var students = _unitOfWork.supervisorRepo.GetStudentsUnderSupervisor(supervisor.AssignmentId);
                var studentDTOs = _mapper.Map<List<StudentDTO>>(students);

                return Ok(studentDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("instructors")]
        public IActionResult GetInstructors()
        {
            try
            {
                var supervisor = GetCurrentSupervisor();

                if (supervisor == null)
                    return Unauthorized();

                var instructors = _unitOfWork.supervisorRepo.GetInstructorsUnderSupervisor(supervisor.AssignmentId);
                var instructorDTOs = _mapper.Map<List<InstructorDTO>>(instructors);

                return Ok(instructorDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            try
            {
                var supervisor = GetCurrentSupervisor();

                if (supervisor == null)
                    return Unauthorized();

                var courses = _unitOfWork.supervisorRepo.GetCoursesUnderSupervisor(supervisor.AssignmentId);
                var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);

                return Ok(courseDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("exams")]
        public IActionResult GetExams()
        {
            try
            {
                var supervisor = GetCurrentSupervisor();

                if (supervisor == null)
                    return Unauthorized();

                var exams = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);
                var examDTOs = _mapper.Map<List<ExamDTO>>(exams);

                return Ok(examDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("dashboard-data")]
        public IActionResult GetDashboardData()
        {
            try
            {
                var supervisor = GetCurrentSupervisor();

                if (supervisor == null)
                    return Unauthorized();

                var students = _unitOfWork.supervisorRepo.GetStudentsUnderSupervisor(supervisor.AssignmentId);
                var instructors = _unitOfWork.supervisorRepo.GetInstructorsUnderSupervisor(supervisor.AssignmentId);
                var courses = _unitOfWork.supervisorRepo.GetCoursesUnderSupervisor(supervisor.AssignmentId);
                var exams = _unitOfWork.supervisorRepo.GetExamsUnderSupervisor(supervisor.AssignmentId);

                var dashboardData = new
                {
                    supervisor = _mapper.Map<SupervisorDTO>(supervisor),
                    studentCount = students.Count,
                    instructorCount = instructors.Count,
                    courseCount = courses.Count,
                    examCount = exams.Count,
                    upcomingExams = exams.Where(e => e.StartTime > DateTime.Now).Count(),
                    activeExams = exams.Where(e => e.StartTime <= DateTime.Now && e.EndTime >= DateTime.Now).Count(),
                    completedExams = exams.Where(e => e.EndTime < DateTime.Now).Count()
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
