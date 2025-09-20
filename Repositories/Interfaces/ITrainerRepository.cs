using Session_Management_System.DTOs.TrainerDtos;
using Session_Management_System.Models;

namespace Session_Management_System.Repositories.Interfaces
{
    public interface ITrainerRepository
    {
        Task<Session> CreateSessionAsync(Session session);
        Task<List<SessionResponseDto>> GetSessionsByTrainerAsync(int trainerId);
        Task<(int completed, int upcoming)> GetSessionStatsAsync(int trainerId);
        Task<List<SessionResponseDto>> GetApprovedSessionsByTrainerAsync(int trainerId);
        Task<List<SessionResponseDto>> GetPendingSessionsByTrainerAsync(int trainerId);
        Task<bool> HasTimeConflictAsync(int trainerId, DateTime startTime, DateTime endTime);
    }
}
