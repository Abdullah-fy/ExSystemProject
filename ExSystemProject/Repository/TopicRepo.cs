using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class TopicRepo:GenaricRepo<Topic>
    {
        public TopicRepo(ExSystemTestContext constext) : base(constext)
        {

        }
    }
}
