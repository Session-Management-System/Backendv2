using Session_Management_System.DTOs;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Services
{
    
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _repo;

        public SessionService(ISessionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<SessionResponseDto>> GetAllSessionsAsync()
        {
            var sessions = await _repo.GetAllSessionsAsync();
            return sessions.Select(s => new SessionResponseDto
            {
                SessionId = s.SessionId,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Capacity = s.Capacity,
                RemainingCapacity = s.RemainingCapacity,
                IsApproved = s.IsApproved,
                SessionLink = s.SessionLink,
                TrainerId = s.TrainerId,
                TrainerName = $"{s.Trainer?.FirstName} {s.Trainer?.LastName}".Trim()
            }).ToList();
        }

        public async Task<SessionResponseDto?> GetSessionByIdAsync(int id)
        {
            var session = await _repo.GetSessionByIdAsync(id);
            if (session == null) return null;

            return new SessionResponseDto
            {
                SessionId = session.SessionId,
                Title = session.Title,
                Description = session.Description,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Capacity = session.Capacity,
                RemainingCapacity = session.RemainingCapacity,
                IsApproved = session.IsApproved,
                SessionLink = session.SessionLink,
                TrainerId = session.TrainerId,
                TrainerName = $"{session.Trainer?.FirstName} {session.Trainer?.LastName}".Trim()
            };
        }

        public async Task<string> UpdateSessionAsync(SessionUpdateDto dto)
        {
            var session = await _repo.GetSessionByIdAsync(dto.SessionId);
            if (session == null) return "Session not found";

            session.Title = dto.Title ?? session.Title;
            session.Description = dto.Description ?? session.Description;
            session.StartTime = dto.StartTime ?? session.StartTime;
            session.EndTime = dto.EndTime ?? session.EndTime;
            session.Capacity = dto.Capacity ?? session.Capacity;
            session.SessionLink = dto.SessionLink ?? session.SessionLink;

            await _repo.UpdateSessionAsync(session);
            return "Session updated successfully";
        }

        public async Task<string> ApproveSessionAsync(int sessionId, bool approve)
        {
            var session = await _repo.GetSessionByIdAsync(sessionId);
            if (session == null) return "Session not found";

            await _repo.ApproveSessionAsync(sessionId, approve);
            return approve ? "Session approved" : "Session disapproved";
        }

        public async Task<string> DeleteSessionAsync(int sessionId)
        {
            var session = await _repo.GetSessionByIdAsync(sessionId);
            if (session == null) return "Session not found";

            await _repo.DeleteSessionAsync(sessionId);
            return "Session deleted successfully";
        }
    }
}
