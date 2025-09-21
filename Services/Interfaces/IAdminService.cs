using Session_Management_System.Models;

namespace Session_Management_System.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<Session>> GetPendingSessionsAsync();
        Task<bool> ApproveSessionAsync(int id);
        Task<bool> RejectSessionAsync(int id);
        Task<object> UserCountStats();
    }
}