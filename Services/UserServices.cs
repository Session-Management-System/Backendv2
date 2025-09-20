using Session_Management_System.DTOs;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _service;

        public UserService(IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            _userRepository = userRepository;
            _service = sessionRepository;
        }

        public async Task<string> BookSessionAsync(int userId, int sessionId)
        {
            if (await _userRepository.IsSessionFullAsync(sessionId))
                    return "Session is already full.";
                    
            var session = await _service.GetSessionByIdAsync(sessionId);

            if (await _userRepository.HasTimeConflictAsync(userId, session.StartTime, session.EndTime))
                return "You already have a session in this time slot.";

            return (await _userRepository.BookSessionAsync(userId, sessionId)).ToString();
        }

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
