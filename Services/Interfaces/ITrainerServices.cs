using Session_Management_System.DTOs.TrainerDtos;

namespace Session_Management_System.Services.Interfaces
{
    public interface ITrainerService
    {
        Task<string> CreateSessionAsync(int trainerId, SessionDto dto);
        Task<List<SessionResponseDto>> GetMySessionsAsync(int trainerId);
        Task<SessionStatsDto> GetSessionStatsAsync(int trainerId);
        Task<List<SessionResponseDto>> GetApprovedSessionsByTrainerAsync(int trainerId);
        Task<List<SessionResponseDto>> GetPendingSessionsByTrainerAsync(int trainerId);
    }
}
