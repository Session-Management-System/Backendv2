using Session_Management_System.DTOs;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Services
{
    public class UserService : AuthService,IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _service;
        private readonly IAuthRepository _authRepo;
        private readonly IEmailService _mailService;

        public UserService(
            IUserRepository userRepository,
            ISessionRepository sessionRepository,
            IAuthRepository authRepository,
            IConfiguration configuration,
            IEmailService emailService
        ) : base(authRepository, configuration, emailService)
        {
            _userRepository = userRepository;
            _service = sessionRepository;
            _authRepo = authRepository;
            _mailService = emailService;
        }

        public async Task<string> BookSessionAsync(int userId, int sessionId)
        {
            if (await _userRepository.IsSessionFullAsync(sessionId))
                return "Session is already full.";

            var session = await _service.GetSessionByIdAsync(sessionId);

            if (await _userRepository.HasTimeConflictAsync(userId, session.StartTime, session.EndTime))
                return "You already have a session in this time slot.";

            UserDetails user = await _userRepository.GetUserDetailsAsync(userId);
            await _mailService.SendEmailAsync(user.Email, "Session Booking Confirmation",
                $"Your session '{session.Title}' has been Booked.<br/>" +
                $"Date: {session.StartTime} - {session.EndTime}<br/>" +
                $"Description: {session.Description}</br>" +
                $"You can Join the Session by loging into the portal!!!");

            return (await _userRepository.BookSessionAsync(userId, sessionId)).ToString();
        }

        public Task<bool> CancelBookingAsync(int bookingId, int userId) =>
            _userRepository.CancelBookingAsync(bookingId, userId);

        public Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId) => _userRepository.GetUserBookingsAsync(userId);

        public Task<IEnumerable<BookingResponseDto>> GetUpcomingBookingsAsync(int userId) =>
            _userRepository.GetUpcomingBookingsAsync(userId);

        public Task<IEnumerable<CompletedSessionDto>> GetCompletedSessionsAsync(int userId) =>
            _userRepository.GetCompletedSessionsAsync(userId);

        public Task<UserSessionStatsDto> GetUserStatsAsync(int userId) =>
            _userRepository.GetUserStatsAsync(userId);

        public Task<IEnumerable<SessionResponseDto>> GetAvailableSessionsAsync(int userId) =>
            _userRepository.GetAvailableSessionsAsync(userId);

        public Task<UserDetails> GetUserDetailsAsync(int userId) =>
            _userRepository.GetUserDetailsAsync(userId);

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto userdetails)
        {
            if (userdetails.OldPassword == null || userdetails.NewPassword == null)
            {
                return await _userRepository.UpdateUserProfileAsync(userId, userdetails.FirstName, userdetails.LastName, userdetails.Email);
            }
            else
            {
                var user = await _authRepo.GetUserByEmailAsync(userdetails.Email);

                if (user == null || !VerifyPassword(userdetails.OldPassword, user.PasswordHash))
                    throw new InvalidOperationException("Invalid credentials");

                // hash the new password
                var newPasswordHash = HashPassword(userdetails.NewPassword);

                return await _userRepository.UpdateUserProfileAsync(
                    userId,
                    userdetails.FirstName,
                    userdetails.LastName,
                    userdetails.Email,
                    newPasswordHash
                );
            }
        }
    }
}
