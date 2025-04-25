using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class StudentExamRepo:GenaricRepo<StudentExam>
    {
        ExSystemTestContext _context;
        public StudentExamRepo(ExSystemTestContext context):base(context) 
        {
           _context = context;
        }
        public List<StudentExam> getStudentExamByStudentAndCourse(int studentId, int courseId)
        {
            var result = _context.StudentExams.FromSqlRaw("EXEC GetStudentExamByStudentAndCourse @studentId, @courseId",
                    new SqlParameter("@studentId",  studentId),
                    new SqlParameter("@courseId", courseId)
                    ).ToList();
            return result;
        }


        public IEnumerable<StudentExam> getActiveById(int examId)
        {
            return _context.StudentExams
                .Where(se => se.ExamId == examId && se.Isactive == true)
                .ToList();
        }
    }
}
