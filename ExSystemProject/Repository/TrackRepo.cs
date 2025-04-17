using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class TrackRepo:GenaricRepo<Track>
    {
        public ExSystemTestContext context { get; }
    
        public TrackRepo(ExSystemTestContext context) : base(context)
        {
            this.context = context;
        }
        public List<Track> GetAllActive()
        {
            return context.Tracks.Where(s => s.IsActive == true).ToList() ;
        }
        public bool Exists(int id)
        {
            return context.Tracks.Any(e => e.TrackId == id);
        }
    }
}
