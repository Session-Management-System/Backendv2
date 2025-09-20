using Microsoft.Data.SqlClient;
using Session_Management_System.DTOs.TrainerDtos;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;

namespace Session_Management_System.Repositories
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly string _connectionString;

        public TrainerRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<Session> CreateSessionAsync(Session session)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Sessions (Title, Description, Capacity, TrainerId, StartTime, EndTime, IsApproved, SessionLink)
                OUTPUT INSERTED.SessionId
                VALUES (@Title, @Description, @Capacity, @TrainerId, @StartTime, @EndTime, @IsApproved, @SessionLink)", conn);

            cmd.Parameters.AddWithValue("@Title", session.Title);
            cmd.Parameters.AddWithValue("@Description", session.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Capacity", session.Capacity);
            cmd.Parameters.AddWithValue("@TrainerId", session.TrainerId);
            cmd.Parameters.AddWithValue("@StartTime", session.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", session.EndTime);
            cmd.Parameters.AddWithValue("@IsApproved", session.IsApproved);
            cmd.Parameters.AddWithValue("@SessionLink", session.SessionLink ?? (object)DBNull.Value);

            await conn.OpenAsync();
            session.SessionId = (int)await cmd.ExecuteScalarAsync();
            return session;
        }

        public async Task<List<SessionResponseDto>> GetSessionsByTrainerAsync(int trainerId)
        {
            var sessions = new List<SessionResponseDto>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime, s.Capacity, s.IsApproved, s.SessionLink,
                       ISNULL((SELECT COUNT(*) FROM Bookings b WHERE b.SessionId = s.SessionId), 0) AS BookedCount,
                       s.TrainerId
                FROM Sessions s
                WHERE s.TrainerId = @TrainerId
                ORDER BY s.StartTime", conn);

            cmd.Parameters.AddWithValue("@TrainerId", trainerId);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sessions.Add(new SessionResponseDto
                {
                    SessionId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.GetDateTime(4),
                    Capacity = reader.GetInt32(5),
                    IsApproved = reader.GetBoolean(6),
                    SessionLink = reader.IsDBNull(7) ? null : reader.GetString(7),
                    RemainingCapacity = reader.GetInt32(5) - reader.GetInt32(8),
                    TrainerId = reader.GetInt32(9)
                });
            }

            return sessions;
        }

        public async Task<(int completed, int upcoming)> GetSessionStatsAsync(int trainerId)
        {
            int completed = 0, upcoming = 0;
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT 
                    SUM(CASE WHEN EndTime < GETUTCDATE() THEN 1 ELSE 0 END) AS Completed,
                    SUM(CASE WHEN StartTime >= GETUTCDATE() THEN 1 ELSE 0 END) AS Upcoming
                FROM Sessions
                WHERE TrainerId = @TrainerId", conn);

            cmd.Parameters.AddWithValue("@TrainerId", trainerId);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                completed = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                upcoming = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            }

            return (completed, upcoming);
        }

        public Task<List<SessionResponseDto>> GetApprovedSessionsByTrainerAsync(int trainerId)
            => GetSessionsByApprovalAsync(trainerId, true);

        public Task<List<SessionResponseDto>> GetPendingSessionsByTrainerAsync(int trainerId)
            => GetSessionsByApprovalAsync(trainerId, false);

        private async Task<List<SessionResponseDto>> GetSessionsByApprovalAsync(int trainerId, bool isApproved)
        {
            var sessions = new List<SessionResponseDto>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime, s.Capacity, s.IsApproved, s.SessionLink,
                       ISNULL((SELECT COUNT(*) FROM Bookings b WHERE b.SessionId = s.SessionId), 0) AS BookedCount,
                       s.TrainerId
                FROM Sessions s
                WHERE s.TrainerId = @TrainerId AND s.IsApproved = @IsApproved
                ORDER BY s.StartTime", conn);

            cmd.Parameters.AddWithValue("@TrainerId", trainerId);
            cmd.Parameters.AddWithValue("@IsApproved", isApproved);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sessions.Add(new SessionResponseDto
                {
                    SessionId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.GetDateTime(4),
                    Capacity = reader.GetInt32(5),
                    IsApproved = reader.GetBoolean(6),
                    SessionLink = reader.IsDBNull(7) ? null : reader.GetString(7),
                    RemainingCapacity = reader.GetInt32(5) - reader.GetInt32(8),
                    TrainerId = reader.GetInt32(9)
                });
            }

            return sessions;
        }

        public async Task<bool> HasTimeConflictAsync(int trainerId, DateTime startTime, DateTime endTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT COUNT(*) 
                FROM Sessions
                WHERE TrainerId = @TrainerId
                AND StartTime < @EndTime 
                AND @StartTime < EndTime";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
    }
}
