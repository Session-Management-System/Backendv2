using Session_Management_System.DTOs;

namespace Session_Management_System.Services.Interfaces
{
    public interface ISessionService
    {
        Task<List<SessionResponseDto>> GetAllSessionsAsync();
        Task<SessionResponseDto?> GetSessionByIdAsync(int id);
        Task<string> UpdateSessionAsync(SessionUpdateDto dto);
        Task<string> ApproveSessionAsync(int sessionId, bool approve);
        Task<string> DeleteSessionAsync(int sessionId);
    }
}
