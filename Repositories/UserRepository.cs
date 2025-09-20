using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Abstractions;
using Session_Management_System.DTOs;
using Session_Management_System.Repositories.Interfaces;

namespace Session_Management_System.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> BookSessionAsync(int userId, int sessionId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Prevent duplicate booking
            var checkQuery = "SELECT COUNT(*) FROM Bookings WHERE UserId = @UserId AND SessionId = @SessionId";
            using (var checkCmd = new SqlCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                checkCmd.Parameters.AddWithValue("@SessionId", sessionId);
                int exists = (int)await checkCmd.ExecuteScalarAsync();
                if (exists > 0) throw new Exception("User already booked this session.");
            }

            var query = "INSERT INTO Bookings (UserId, SessionId) OUTPUT INSERTED.BookingId VALUES (@UserId, @SessionId)";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@SessionId", sessionId);

                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM Bookings WHERE BookingId = @BookingId AND UserId = @UserId";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@BookingId", bookingId);
                command.Parameters.AddWithValue("@UserId", userId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId)
        {
            return await FetchBookings(userId, ""); // all bookings
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUpcomingBookingsAsync(int userId)
        {
            return await FetchBookings(userId, "AND s.StartTime > GETDATE()");
        }

        private async Task<IEnumerable<BookingResponseDto>> FetchBookings(int userId, string extraCondition)
        {
            var bookings = new List<BookingResponseDto>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                SELECT b.BookingId,
                       s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime,
                       s.Capacity, s.IsApproved, s.SessionLink, s.TrainerId,
                       u.FirstName, u.LastName,
                       (s.Capacity - COUNT(b2.BookingId)) AS RemainingCapacity
                FROM Bookings b
                JOIN Sessions s ON b.SessionId = s.SessionId
                JOIN Users u ON s.TrainerId = u.UserId
                LEFT JOIN Bookings b2 ON s.SessionId = b2.SessionId
                WHERE b.UserId = @UserId {extraCondition}
                GROUP BY b.BookingId, s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime,
                         s.Capacity, s.IsApproved, s.SessionLink, s.TrainerId, u.FirstName, u.LastName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookings.Add(new BookingResponseDto
                {
                    BookingId = reader.GetInt32(0),
                    SessionId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    StartTime = reader.GetDateTime(4),
                    EndTime = reader.GetDateTime(5),
                    Capacity = reader.GetInt32(6),
                    IsApproved = reader.GetBoolean(7),
                    SessionLink = reader.IsDBNull(8) ? null : reader.GetString(8),
                    TrainerId = reader.GetInt32(9),
                    TrainerName = $"{reader.GetString(10)} {reader.GetString(11)}",
                    RemainingCapacity = reader.GetInt32(12)
                });
            }

            return bookings;
        }

        public async Task<IEnumerable<CompletedSessionDto>> GetCompletedSessionsAsync(int userId)
        {
            var sessions = new List<CompletedSessionDto>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime,
                       s.Capacity, s.SessionLink, u.FirstName, u.LastName,
                       (s.Capacity - COUNT(b.BookingId)) AS RemainingCapacity
                FROM Bookings b
                JOIN Sessions s ON b.SessionId = s.SessionId
                JOIN Users u ON s.TrainerId = u.UserId
                WHERE b.UserId = @UserId AND s.EndTime < GETDATE()
                GROUP BY s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime,
                         s.Capacity, s.SessionLink, u.FirstName, u.LastName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sessions.Add(new CompletedSessionDto
                {
                    SessionId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.GetDateTime(4),
                    Capacity = reader.GetInt32(5),
                    SessionLink = reader.IsDBNull(6) ? null : reader.GetString(6),
                    TrainerName = $"{reader.GetString(7)} {reader.GetString(8)}",
                    RemainingCapacity = reader.GetInt32(9)
                });
            }

            return sessions;
        }

        public async Task<UserSessionStatsDto> GetUserStatsAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var stats = new UserSessionStatsDto();

            var query = @"
                SELECT 
                    (SELECT COUNT(*) FROM Bookings WHERE UserId = @UserId) AS TotalBookings,
                    (SELECT COUNT(*) FROM Bookings b JOIN Sessions s ON b.SessionId = s.SessionId 
                        WHERE b.UserId = @UserId AND s.EndTime < GETDATE()) AS CompletedSessions,
                    (SELECT COUNT(*) FROM Bookings b JOIN Sessions s ON b.SessionId = s.SessionId 
                        WHERE b.UserId = @UserId AND s.StartTime > GETDATE()) AS UpcomingSessions";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                stats.TotalBookings = reader.GetInt32(0);
                stats.CompletedSessions = reader.GetInt32(1);
                stats.UpcomingSessions = reader.GetInt32(2);
            }

            return stats;
        }

        public async Task<IEnumerable<SessionResponseDto>> GetAvailableSessionsAsync(int userId)
        {
            var sessions = new List<SessionResponseDto>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime,
                   s.Capacity, (s.Capacity - COUNT(b.BookingId)) AS RemainingCapacity,
                   s.SessionLink, t.UserId, CONCAT(t.FirstName, t.LastName) AS TrainerName
            FROM Sessions s
            INNER JOIN Users t ON s.TrainerId = t.UserId
            LEFT JOIN Bookings b ON s.SessionId = b.SessionId
            WHERE s.SessionId NOT IN (
                SELECT SessionId FROM Bookings WHERE UserId = @UserId
            )
            GROUP BY s.SessionId, s.Title, s.Description, s.StartTime, s.EndTime,
                     s.Capacity, s.SessionLink, t.UserId, t.FirstName, t.LastName";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sessions.Add(new SessionResponseDto
                            {
                                SessionId = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                StartTime = reader.GetDateTime(3),
                                EndTime = reader.GetDateTime(4),
                                Capacity = reader.GetInt32(5),
                                RemainingCapacity = reader.GetInt32(6),
                                SessionLink = reader.GetString(7),
                                TrainerId = reader.GetInt32(8),
                                TrainerName = reader.GetString(9)
                            });
                        }
                    }
                }
            }
            return sessions;
        }
        public async Task<bool> IsSessionFullAsync(int sessionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get current count of bookings
                string countQuery = @"SELECT COUNT(*) 
                              FROM Bookings 
                              WHERE SessionId = @SessionId";

                using (var cmd = new SqlCommand(countQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    int bookingCount = (int)await cmd.ExecuteScalarAsync();

                    // Get session capacity
                    string capacityQuery = @"SELECT Capacity 
                                     FROM Sessions 
                                     WHERE SessionId = @SessionId";

                    using (var cmdCap = new SqlCommand(capacityQuery, connection))
                    {
                        cmdCap.Parameters.AddWithValue("@SessionId", sessionId);
                        int capacity = (int)await cmdCap.ExecuteScalarAsync();

                        return bookingCount >= capacity;
                    }
                }
            }
        }

        public async Task<bool> HasTimeConflictAsync(int userId, DateTime startTime, DateTime endTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT COUNT(*) 
                FROM Bookings b
                INNER JOIN Sessions s ON b.SessionId = s.SessionId
                WHERE b.UserId = @UserId
                AND s.StartTime < @EndTime 
                AND @StartTime < s.EndTime";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
    }
}
