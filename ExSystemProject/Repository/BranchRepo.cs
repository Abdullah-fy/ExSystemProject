using ExSystemProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExSystemProject.Repository
{
    public class BranchRepo : GenaricRepo<Branch>
    {
        private readonly ExSystemTestContext context;

        public BranchRepo(ExSystemTestContext context) : base(context)
        {
            this.context = context;
        }
        public List<Branch> GetAllActive()
        {
            return context.Branches.Where(a => a.Isactive == true).ToList();
        }
        public bool Exists(int id)
        {
            return context.Branches.Any(e => e.BranchId == id);
        }

        public List<Track> GetTracksByBranchId(int id)
        {
            return context.Tracks
             .FromSqlRaw("EXEC sp_GetTracksByBranchId @BranchId", new SqlParameter("@BranchId", id))
             .ToList();
        }

        public BranchDTO GetBranchById(int branchId)
        {
            var branch = new BranchDTO();

            try
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "sp_GetBranchById";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@branch_id";
                param.Value = branchId;
                param.DbType = DbType.Int32;
                command.Parameters.Add(param);

                context.Database.OpenConnection();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    branch = new BranchDTO
                    {
                        branch_id = reader["branch_id"] != DBNull.Value ? Convert.ToInt32(reader["branch_id"]) : 0,
                        branch_name = reader["branch_name"]?.ToString(),
                        location = reader["location"]?.ToString(),
                        isactive = reader["isactive"] != DBNull.Value ? Convert.ToBoolean(reader["isactive"]) : null
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBranchById: {ex.Message}");
                throw; 
            }
            finally
            {
                context.Database.CloseConnection();
            }

            return branch;
        }

    }
}