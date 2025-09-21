using Session_Management_System.DTOs;

namespace Session_Management_System.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> BookSessionAsync(int userId, int sessionId);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<BookingResponseDto>> GetUpcomingBookingsAsync(int userId);
        Task<IEnumerable<CompletedSessionDto>> GetCompletedSessionsAsync(int userId);
        Task<UserSessionStatsDto> GetUserStatsAsync(int userId);
        Task<IEnumerable<SessionResponseDto>> GetAvailableSessionsAsync(int userId);
        Task<UserDetails> GetUserDetailsAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto userdetails);
    }
}
