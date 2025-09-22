using Microsoft.VisualBasic;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;
using Session_Management_System.Services.Interfaces;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;
    private readonly ISessionRepository _sessionrepo;
    private readonly IEmailService _emailservice;

    public AdminService(IAdminRepository repo, ISessionRepository sessionrepo, IEmailService emailservice)
    {
        _repo = repo;
        _sessionrepo = sessionrepo;
        _emailservice = emailservice;
    }

    public Task<IEnumerable<Session>> GetPendingSessionsAsync() => _repo.GetPendingSessionsAsync();
    public async Task<bool> ApproveSessionAsync(int id)
    {
        string email = await _repo.GetEmailId(id);
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException($"No email found for session {id}");
        }
        Session sessionDetails = await _sessionrepo.GetSessionByIdAsync(id);
        await _emailservice.SendEmailAsync(email, "Session Approved by Admin",
        $"Your session '{sessionDetails.Title}' has been Approved.<br/>" +
        $"Date: {sessionDetails.StartTime} - {sessionDetails.EndTime}<br/>Capacity: {sessionDetails.Capacity}<br/>" +
        $"Description: {sessionDetails.Description}</br> Session Link: {sessionDetails.SessionLink}");

        return await _repo.ApproveSessionAsync(id);
    }
    public async Task<bool> RejectSessionAsync(int id, string comment)
    {
        string email = await _repo.GetEmailId(id);
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException($"No email found for session {id}");
        }
        Session sessionDetails = await _sessionrepo.GetSessionByIdAsync(id);
        await _emailservice.SendEmailAsync(email, "Session Rejected by Admin",
        $"Your session '{sessionDetails.Title}' has been Rejected.<br/>" +
        $"Date: {sessionDetails.StartTime} - {sessionDetails.EndTime}<br/>Capacity: {sessionDetails.Capacity}<br/>" +
        $"Description: {sessionDetails.Description}</br> Session Link: {sessionDetails.SessionLink}<br/>Comment: {comment}");

        return await _repo.RejectSessionAsync(id);
    }
    public Task<object> UserCountStats() => _repo.UserCountStatsAsync();
}
