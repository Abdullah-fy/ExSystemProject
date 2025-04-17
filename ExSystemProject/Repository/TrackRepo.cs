using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class TrackRepo : GenaricRepo<Track>
    {
        public ExSystemTestContext context { get; }

        public TrackRepo(ExSystemTestContext context) : base(context)
        {
            this.context = context;
        }
        public List<Track> GetAllActive()
        {
            return context.Tracks.Where(s => s.IsActive == true).ToList();
        }
        public bool Exists(int id)
        {
            return context.Tracks.Any(e => e.TrackId == id);
        }
        public List<TrackDTO> GetAllWithBranch()
        {
            List<Track> tracks = context.Tracks.Include(t => t.Branch).ToList();
            List<TrackDTO> trackDTO = new List<TrackDTO>();
            foreach (var track in tracks)
            {

                trackDTO.Add(new TrackDTO()
                {
                    TrackId = track.TrackId,
                    TrackName = track.TrackName,
                    TrackDuration = track.TrackDuration,
                    TrackIntake = track.TrackIntake,
                    IsActive = track.IsActive,
                    BranchId = track.BranchId,
                    BranchName = track.Branch.BranchName
                });
            }

            return trackDTO;
        }
    }
}