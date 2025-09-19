using Session_Management_System.DTOs;

namespace Session_Management_System.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<int> BookSessionAsync(int userId, int sessionId);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<BookingResponseDto>> GetUpcomingBookingsAsync(int userId);
        Task<IEnumerable<CompletedSessionDto>> GetCompletedSessionsAsync(int userId);
        Task<UserSessionStatsDto> GetUserStatsAsync(int userId);
        Task<IEnumerable<SessionResponseDto>> GetAvailableSessionsAsync(int userId);
    }
}
