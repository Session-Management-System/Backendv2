using Session_Management_System.Models;

namespace Session_Management_System.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<(Session Session, string TrainerName)>> GetPendingSessionsAsync();
        Task<bool> ApproveSessionAsync(int id);
        Task<bool> RejectSessionAsync(int id, string comment);
        Task<object> UserCountStatsAsync();
        Task<IEnumerable<User>> GetUserDetailsAsync(int user);
        Task<(int totalSessions, int completedSessions)> GetSessionStatsAsync();
    }
}