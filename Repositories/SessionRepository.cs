using Microsoft.Data.SqlClient;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;

namespace Session_Management_System.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly string _connectionString;

        public SessionRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Session>> GetAllSessionsAsync()
        {
            var sessions = new List<Session>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"SELECT s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime, s.Capacity, s.IsApproved, s.SessionLink, s.TrainerId,
                         u.FirstName, u.LastName,
                         COUNT(b.SessionId) AS BookedCount
                  FROM Sessions s
                  LEFT JOIN Users u ON s.TrainerId = u.UserId
                  LEFT JOIN Bookings b ON s.SessionId = b.SessionId
                  GROUP BY s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime, s.Capacity, s.IsApproved, s.SessionLink, s.TrainerId, u.FirstName, u.LastName
                  ORDER BY s.StartTime DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var capacity = reader.GetInt32(5);
                var bookedCount = reader.GetInt32(11);

                sessions.Add(new Session
                {
                    SessionId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.GetDateTime(4),
                    Capacity = capacity,
                    RemainingCapacity = capacity - bookedCount,
                    IsApproved = reader.GetBoolean(6),
                    SessionLink = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TrainerId = reader.GetInt32(8),
                    Trainer = new User
                    {
                        UserId = reader.GetInt32(8),
                        FirstName = reader.GetString(9),
                        LastName = reader.GetString(10)
                    }
                });
            }
            return sessions;
        }

        public async Task<Session?> GetSessionByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"SELECT s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime, s.Capacity, s.IsApproved, s.SessionLink, s.TrainerId,
                         u.FirstName, u.LastName,
                         COUNT(b.SessionId) AS BookedCount
                  FROM Sessions s
                  LEFT JOIN Users u ON s.TrainerId = u.UserId
                  LEFT JOIN Bookings b ON s.SessionId = b.SessionId
                  WHERE s.SessionId = @SessionId
                  GROUP BY s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime, s.Capacity, s.IsApproved, s.SessionLink, s.TrainerId, u.FirstName, u.LastName", conn);

            cmd.Parameters.AddWithValue("@SessionId", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var capacity = reader.GetInt32(5);
                var bookedCount = reader.GetInt32(11);

                return new Session
                {
                    SessionId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.GetDateTime(4),
                    Capacity = capacity,
                    RemainingCapacity = capacity - bookedCount,
                    IsApproved = reader.GetBoolean(6),
                    SessionLink = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TrainerId = reader.GetInt32(8),
                    Trainer = new User
                    {
                        UserId = reader.GetInt32(8),
                        FirstName = reader.GetString(9),
                        LastName = reader.GetString(10)
                    }
                };
            }

            return null;
        }

        public async Task UpdateSessionAsync(Session session)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"UPDATE Sessions
                  SET Title = @Title, Description = @Description, StartTime = @StartTime, EndTime = @EndTime,
                      Capacity = @Capacity, SessionLink = @SessionLink
                  WHERE SessionId = @SessionId", conn);

            cmd.Parameters.AddWithValue("@Title", session.Title);
            cmd.Parameters.AddWithValue("@Description", session.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StartTime", session.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", session.EndTime);
            cmd.Parameters.AddWithValue("@Capacity", session.Capacity);
            cmd.Parameters.AddWithValue("@SessionLink", session.SessionLink ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SessionId", session.SessionId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("DELETE FROM Sessions WHERE SessionId = @SessionId", conn);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
