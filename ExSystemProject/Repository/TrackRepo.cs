using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    public class TrackRepo:GenaricRepo<Track>
    {
        public TrackRepo(ExSystemTestContext constext) : base(constext)
        {

        }
    }
}
