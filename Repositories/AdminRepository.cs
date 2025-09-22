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

        public async Task<IEnumerable<(Session Session, string TrainerName)>> GetPendingSessionsAsync()
        {
            var results = new List<(Session, string)>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
            SELECT s.SessionId, s.Title, s.StartTime, s.EndTime, 
                   s.Capacity, s.TrainerId, s.IsApproved,
                   u.FirstName, u.LastName
            FROM Sessions s
            INNER JOIN Users u ON s.TrainerId = u.UserId
            WHERE s.IsApproved = 0";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var session = new Session
                        {
                            SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                            EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
                            Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                            TrainerId = reader.GetInt32(reader.GetOrdinal("TrainerId")),
                            IsApproved = reader.GetBoolean(reader.GetOrdinal("IsApproved"))
                        };

                        string trainerName =
                            $"{reader.GetString(reader.GetOrdinal("FirstName"))} {reader.GetString(reader.GetOrdinal("LastName"))}";

                        results.Add((session, trainerName));
                    }
                }
            }

            return results;
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

        public async Task<IEnumerable<User>> GetUserDetailsAsync(int roleId)
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"SELECT UserId, FirstName, LastName, Email 
                                FROM Users 
                                WHERE RoleId = @roleId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    // Add parameter safely
                    cmd.Parameters.AddWithValue("@roleId", roleId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                            });
                        }
                    }
                }
                return users;
            }
        }
        public async Task<(int totalSessions, int completedSessions)> GetSessionStatsAsync()
        {
            int total = 0, completed = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                        SELECT 
                        COUNT(*) AS TotalSessions,
                        SUM(CASE WHEN s.EndTime < GETDATE() THEN 1 ELSE 0 END) AS CompletedSessions
                        FROM Sessions s;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            total = reader.GetInt32(0);
                            completed = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }
                    }
                }
            }

            return (total, completed);
        }
    }
}