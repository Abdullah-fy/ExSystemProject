using ExSystemProject.Models;
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
    }
}
