using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ExSystemProject.Repository
{
    public class TrackRepo:GenaricRepo<Track>
    {
        ExSystemTestContext _context;
        public TrackRepo(ExSystemTestContext constext) : base(constext)
        {
            this._context = constext;
        }


        public List<TrackDTO> GetAllWithBranch()
        {
            List<Track> tracks = _context.Tracks.Include(t => t.Branch).ToList();
            List<TrackDTO> trackDTO = new List<TrackDTO>();
            foreach (var track in tracks) {

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
