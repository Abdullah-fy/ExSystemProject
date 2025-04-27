using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        public List<Track> GetTracksByBranchId(int branchId)
        {
            try
            {
                // Use LINQ to query tracks by branch ID, including both active and inactive
                var tracks = context.Tracks
                             .Where(t => t.BranchId == branchId)
                             .Include(t => t.Branch)
                             .ToList();

                return tracks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTracksByBranchId: {ex.Message}");
                return new List<Track>();
            }
        }

        public List<Track> GetActiveTracksByBranchId(int branchId)
        {
            try
            {
                // Use LINQ to query only active tracks by branch ID
                var tracks = context.Tracks
                             .Where(t => t.BranchId == branchId && t.IsActive == true)
                             .Include(t => t.Branch)
                             .ToList();

                return tracks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetActiveTracksByBranchId: {ex.Message}");
                return new List<Track>();
            }
        }

        public int GetTrackCountByBranchAsync(int branchId)
        {
            return context.Tracks
                .Where(t => t.BranchId == branchId && t.IsActive == true)
                .Count();
        }
    }
}
