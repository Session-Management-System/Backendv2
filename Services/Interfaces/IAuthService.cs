using Session_Management_System.DTOs;

namespace Session_Management_System.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterDto dto);
        Task<AuthResponse> LoginAsync(LoginDto dto);
        Task<string> GenerateAndSendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otpCode);
        Task<bool> UpdatePasswordAsync(string email, string newPassword);
    }
}
