using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        [HttpGet("pending-sessions")]
        public async Task<IActionResult> GetPendingApprovelSessions()
        {
            var sessions = await _service.GetPendingSessionsAsync();
            return Ok(sessions);
        }

        [HttpPost("approve-session/{id}")]
        public async Task<IActionResult> ApproveSession(int id)
        {
            var success = await _service.ApproveSessionAsync(id);
            return success ? Ok(new { Message = "Session approved." }) : NotFound("Session not found.");
        }

        [HttpPost("reject-session/{id}")]
        public async Task<IActionResult> RejectSession(int id, [FromBody] string comment)
        {
            var success = await _service.RejectSessionAsync(id, comment);
            return success ? Ok(new { Message = "Session rejected and deleted." }) : NotFound("Session not found.");
        }

        [HttpGet("Active-user-trainers")]
        public async Task<IActionResult> UserCountStats()
        {
            var count = await _service.UserCountStatsAsync();
            return Ok(count);
        }

        [HttpGet("Get-user-Details/{roleId}")]
        public async Task<IActionResult> GetUserDetails(int roleId)
        {
            var User = await _service.GetUserDetailsAsync(roleId);
            return Ok(User);
        }

        [HttpGet("session-stats")]
        public async Task<IActionResult> GetSessionStats()
        {
            var (totalSessions, completedSessions) = await _service.GetSessionStatsAsync();
            return Ok(new
            {
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions
            });
        }

    }
}
