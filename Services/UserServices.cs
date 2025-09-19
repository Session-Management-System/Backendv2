using Session_Management_System.DTOs;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<int> BookSessionAsync(int userId, int sessionId) =>
            _userRepository.BookSessionAsync(userId, sessionId);

        public Task<bool> CancelBookingAsync(int bookingId, int userId) =>
            _userRepository.CancelBookingAsync(bookingId, userId);

        public Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId) =>
            _userRepository.GetUserBookingsAsync(userId);

        public Task<IEnumerable<BookingResponseDto>> GetUpcomingBookingsAsync(int userId) =>
            _userRepository.GetUpcomingBookingsAsync(userId);

        public Task<IEnumerable<CompletedSessionDto>> GetCompletedSessionsAsync(int userId) =>
            _userRepository.GetCompletedSessionsAsync(userId);

        public Task<UserSessionStatsDto> GetUserStatsAsync(int userId) =>
            _userRepository.GetUserStatsAsync(userId);

        public Task<IEnumerable<SessionResponseDto>> GetAvailableSessionsAsync(int userId)=>
            _userRepository.GetAvailableSessionsAsync(userId);

    }
}
