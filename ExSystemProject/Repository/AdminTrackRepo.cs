using ExSystemProject.DTOS;
using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExSystemProject.Repository
{
    public class AdminTrackRepo
    {
        private readonly ExSystemTestContext _context;

        public AdminTrackRepo(ExSystemTestContext context)
        {
            _context = context;
        }

        // Get all active tracks
        public List<Track> GetAllActive()
        {
            return _context.Tracks.Where(s => s.IsActive == true).ToList();
        }

        // Check if track exists
        public bool Exists(int id)
        {
            return _context.Tracks.Any(e => e.TrackId == id);
        }

        // Get all tracks with branch information
        public List<TrackDTO> GetAllWithBranch()
        {
            List<Track> tracks = _context.Tracks.Include(t => t.Branch).ToList();
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

        // Get tracks by branch ID
        public List<Track> GetTracksByBranchId(int branchId)
        {
            try
            {
                var tracks = _context.Tracks
                    .Where(t => t.BranchId == branchId && t.IsActive == true)
                    .ToList();

                return tracks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTracksByBranchId: {ex.Message}");
                return new List<Track>();
            }
        }

        // Create a track
        public void CreateTrack(Track track)
        {
            _context.Tracks.Add(track);
            _context.SaveChanges();
        }

        // Update a track
        public void UpdateTrack(Track track)
        {
            _context.Entry(track).State = EntityState.Modified;
            _context.SaveChanges();
        }

        // Delete a track
        public void DeleteTrack(int id)
        {
            var track = _context.Tracks.Find(id);
            if (track != null)
            {
                _context.Tracks.Remove(track);
                _context.SaveChanges();
            }
        }

        // Get track by ID
        public Track GetTrackById(int id)
        {
            return _context.Tracks.Include(t => t.Branch).FirstOrDefault(t => t.TrackId == id);
        }
    }
}
