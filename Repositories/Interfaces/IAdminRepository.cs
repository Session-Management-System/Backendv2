using Session_Management_System.Models;

namespace Session_Management_System.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<IEnumerable<Session>> GetPendingSessionsAsync();
        Task<bool> ApproveSessionAsync(int sessionId);
        Task<bool> RejectSessionAsync(int sessionId);
        Task<object> UserCountStatsAsync();
    }
}
