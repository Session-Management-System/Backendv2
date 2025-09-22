using Microsoft.Data.SqlClient;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;

namespace Session_Management_System.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionString;

        public AdminRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Session>> GetPendingSessionsAsync()
        {
            var sessions = new List<Session>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"SELECT * FROM Sessions WHERE IsApproved = 0";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sessions.Add(new Session
                        {
                            SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                            EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
                            Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                            TrainerId = reader.GetInt32(reader.GetOrdinal("TrainerId")),
                            IsApproved = reader.GetBoolean(reader.GetOrdinal("IsApproved"))
                        });
                    }
                }
            }
            return sessions;
        }

        public async Task<bool> ApproveSessionAsync(int sessionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE Sessions SET IsApproved = 1 WHERE SessionId = @SessionId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> RejectSessionAsync(int sessionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "DELETE FROM Sessions WHERE SessionId = @SessionId AND IsApproved = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<object> UserCountStatsAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"SELECT 
                                (SELECT COUNT(*) FROM Users WHERE RoleId = '1') AS UserCount,
                                (SELECT COUNT(*) FROM Users WHERE RoleId = '2') AS TrainerCount";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new
                        {
                            UserCount = reader.GetInt32(reader.GetOrdinal("UserCount")),
                            TrainerCount = reader.GetInt32(reader.GetOrdinal("TrainerCount"))
                        };
                    }
                }
            }
            return null;
        }

        public async Task<string> GetEmailId(int sessionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                const string query = @"
                SELECT u.email
                FROM users u
                INNER JOIN sessions s ON u.UserId = s.Trainerid
                WHERE s.SessionId = @SessionId;";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    var result = await cmd.ExecuteScalarAsync();
                    return result == DBNull.Value ? null : result?.ToString();                    
                }
            }
        }
    }
}