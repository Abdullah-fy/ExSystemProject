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

        public List<Track> GetDistictTracks()
        {
            return context.Tracks
         .GroupBy(t => t.TrackName)  // Group by the unique identifier (TrackId)
         .Select(g => g.First())  // Select the first occurrence from each group
         .ToList();
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
                    track_id = track.TrackId,
                    track_name = track.TrackName,
                    track_duration = track.TrackDuration,
                    track_intake = track.TrackIntake,
                   is_active = track.IsActive,
                    branch_id = track.BranchId,
                    branch_name = track.Branch.BranchName
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
        //-------- expand 
        public TrackDTO GetTrackById(int trackId)
        {
            var track = new TrackDTO();

            try
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_GetTrackById";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@track_id";
                param.Value = trackId;
                param.DbType = DbType.Int32;
                command.Parameters.Add(param);

                context.Database.OpenConnection();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    track = new TrackDTO
                    {
                        track_id = reader["track_id"] != DBNull.Value ? Convert.ToInt32(reader["track_id"]) : null,
                        track_name = reader["track_name"]?.ToString(),
                        track_duration = reader["track_duration"] != DBNull.Value ? Convert.ToInt32(reader["track_duration"]) : null,
                        track_intake = reader["track_intake"] != DBNull.Value ? Convert.ToInt32(reader["track_intake"]) : null,
                        is_active = reader["is_active"] != DBNull.Value ? Convert.ToBoolean(reader["is_active"]) : null,
                        branch_id = reader["branch_id"] != DBNull.Value ? Convert.ToInt32(reader["branch_id"]) : null,
                        branch_name = reader["branch_name"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in GetTrackById: {ex.Message}");
                throw; // Re-throw for controller handling
            }
            finally
            {
                context.Database.CloseConnection();
            }

            return track;
        }

        // expand 


        public IEnumerable<StudentByTrackDTO> GetStudentsByTrackId(int track_id)
        {
            var students = new List<StudentByTrackDTO>();

            try
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_GetStudentsByTrackId";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@track_id";
                param.Value = track_id;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                context.Database.OpenConnection();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new StudentByTrackDTO
                    {
                        Studentid = Convert.ToInt32(reader["Studentid"]),
                        track_id = reader["track_id"].ToString(),
                        userid = reader["userid"].ToString(),
                        EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                        isActive = Convert.ToBoolean(reader["isActive"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentsByTrackId: {ex.Message}");
                throw;
            }
            finally
            {
                context.Database.CloseConnection();
            }

            return students;
        }
    }
}
