using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class AdminTopicRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminTopicRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        public Topic GetTopicById(int id)
        {
            var idParam = new SqlParameter("@topic_id", id);
            var result = _context.Topics
                .FromSqlRaw("EXEC sp_GetTopicById @topic_id", idParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result;
        }

        public List<Topic> GetTopicsByCourseId(int courseId, bool? activeOnly = null)
        {
            var courseIdParam = new SqlParameter("@crs_id", courseId);
            var activeParam = activeOnly.HasValue
                ? new SqlParameter("@Getactive", activeOnly.Value)
                : new SqlParameter("@Getactive", DBNull.Value);

            var result = _context.Topics
                .FromSqlRaw("EXEC sp_GetTopicsByCourseId @crs_id, @Getactive",
                    courseIdParam, activeParam)
                .AsEnumerable()
                .ToList();

            return result;
        }

        public Topic CreateTopic(string name, string description, int courseId)
        {
            var nameParam = new SqlParameter("@topic_name", name);
            var descriptionParam = new SqlParameter("@description",
                string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
            var courseIdParam = new SqlParameter("@crs_id", courseId);

            var result = _context.Topics
                .FromSqlRaw("EXEC sp_CreateTopic @topic_name, @description, @crs_id",
                    nameParam, descriptionParam, courseIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result;
        }

        public Topic UpdateTopic(int id, string name, string description, int courseId, bool isActive)
        {
            var idParam = new SqlParameter("@topic_id", id);
            var nameParam = new SqlParameter("@topic_name", name);
            var descriptionParam = new SqlParameter("@description",
                string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
            var courseIdParam = new SqlParameter("@crs_id", courseId);
            var isActiveParam = new SqlParameter("@isactive", isActive);

            var result = _context.Topics
                .FromSqlRaw("EXEC sp_UpdateTopic @topic_id, @topic_name, @description, @crs_id, @isactive",
                    idParam, nameParam, descriptionParam, courseIdParam, isActiveParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result;
        }

        public Topic DeleteTopic(int id)
        {
            var idParam = new SqlParameter("@topic_id", id);

            var result = _context.Topics
                .FromSqlRaw("EXEC sp_DeleteTopic @topic_id", idParam)
                .AsEnumerable()
                .FirstOrDefault();

            return result;
        }
    }
}
