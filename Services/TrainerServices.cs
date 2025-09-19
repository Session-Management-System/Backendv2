using Session_Management_System.DTOs.TrainerDtos;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _repository;

        public TrainerService(ITrainerRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> CreateSessionAsync(int trainerId, SessionDto dto)
        {
            var session = new Session
            {
                Title = dto.Title,
                Description = dto.Description,
                Capacity = dto.Capacity,
                TrainerId = trainerId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsApproved = false
            };

            await _repository.CreateSessionAsync(session);
            return "Session created. Waiting for admin approval.";
        }

        public Task<List<SessionResponseDto>> GetMySessionsAsync(int trainerId) =>
            _repository.GetSessionsByTrainerAsync(trainerId);

        public async Task<SessionStatsDto> GetSessionStatsAsync(int trainerId)
        {
            var (completed, upcoming) = await _repository.GetSessionStatsAsync(trainerId);
            return new SessionStatsDto
            {
                CompletedSessions = completed,
                UpcomingSessions = upcoming
            };
        }

        public Task<List<SessionResponseDto>> GetApprovedSessionsByTrainerAsync(int trainerId) =>
            _repository.GetApprovedSessionsByTrainerAsync(trainerId);

        public Task<List<SessionResponseDto>> GetPendingSessionsByTrainerAsync(int trainerId) =>
            _repository.GetPendingSessionsByTrainerAsync(trainerId);
    }
}
