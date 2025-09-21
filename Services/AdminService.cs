using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;

    public AdminService(IAdminRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Session>> GetPendingSessionsAsync() => _repo.GetPendingSessionsAsync();
    public Task<bool> ApproveSessionAsync(int id) => _repo.ApproveSessionAsync(id);
    public Task<bool> RejectSessionAsync(int id) => _repo.RejectSessionAsync(id);
    public Task<object> UserCountStats() => _repo.UserCountStatsAsync();
}
