using Session_Management_System.Models;

namespace Session_Management_System.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        Task<List<Session>> GetAllSessionsAsync();
        Task<Session?> GetSessionByIdAsync(int id);
        Task UpdateSessionAsync(Session session);
        Task DeleteSessionAsync(int sessionId);
    }
}
