using ExSystemProject.Models;
using ExSystemProject.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace ExSystemProject.Repository
{
    public class StudentRepo:GenaricRepo<Student>
    {
        ExSystemTestContext _context;

        public StudentRepo(ExSystemTestContext context ) : base(context)
        {
            this._context = context;
         }
        public Student getByUserId(int userId)
        {
            var result = _context.Students.FirstOrDefault(i => i.UserId == userId && i.Isactive == true);
            return result;
        }


        public void AddNewStudent(StudentViewModel model)
        {
            _context.Database.ExecuteSqlRaw(
            "EXEC sp_CreateStudent @p0, @p1, @p2, @p3, @p4, @p5",
            model.username, model.Email, model.Gender, model.password, model.TrackId, model.Image);
        }

        public List<Student> GetStudentByInstructorAndCourse(int instructorId, int courseId)
        {
            var students = _context.Students
                .FromSqlRaw("EXEC GetStudentsByCourseAndInstructor @CourseId, @InstructorId",
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@InstructorId", instructorId))
                .ToList();

            return students;
        }

          
    }
}
