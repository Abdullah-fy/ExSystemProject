using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ExSystemProject.Repository
{
    public class InstructorRepo:GenaricRepo<Instructor>
    {
        ExSystemTestContext _context;
        public InstructorRepo(ExSystemTestContext context):base(context)
        {
            _context = context;
        }

       public List<InstructorDTO> getAllWithUserData()
        {
            List<Instructor> instructors = _context.Instructors.Include(i => i.User).ToList();
            List<InstructorDTO> insDto = new List<InstructorDTO>();

            foreach (var item in instructors)
            {
                insDto.Add(new InstructorDTO
                {
                    InsId = item.InsId,
                    Username = item.User.Username,
                    UserId = item.User.UserId
                });
                
            }
            return insDto;
        }
        public List<Student> GetStudentsByInstructor(int instructorId)
        {
            return _context.StudentCourses.Where(sc => sc.Crs.InsId == instructorId && sc.Isactive == true).Select(sc => sc.Student).Distinct().Include(a => a.Track).Include(b => b.User).ToList();
        }
        public Instructor getByUserId(int userId)
        {
            return _context.Instructors.FirstOrDefault(i => i.UserId == userId && i.Isactive == true);
        }

    }
}
