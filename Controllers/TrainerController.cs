using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Session_Management_System.DTOs.TrainerDtos;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Controllers
{
    [Authorize(Roles = "Trainer")]
    [ApiController]
    [Route("api/[controller]")]
    public class TrainerController : ControllerBase
    {
        private readonly ITrainerService _trainerService;

        public TrainerController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        private int GetTrainerId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession(SessionDto dto)
        {
            var message = await _trainerService.CreateSessionAsync(GetTrainerId(), dto);
            return Ok(new { Message = message });
        }

        [HttpGet("my-sessions")]
        public async Task<IActionResult> GetMySessions()
        {
            var sessions = await _trainerService.GetMySessionsAsync(GetTrainerId());
            return Ok(sessions);
        }

        [HttpGet("session-stats")]
        public async Task<IActionResult> GetSessionStats()
        {
            var stats = await _trainerService.GetSessionStatsAsync(GetTrainerId());
            return Ok(stats);
        }

        [HttpGet("approved-sessions")]
        public async Task<IActionResult> GetApprovedSessions()
        {
            var sessions = await _trainerService.GetApprovedSessionsByTrainerAsync(GetTrainerId());
            return Ok(sessions);
        }

        [HttpGet("pending-sessions")]
        public async Task<IActionResult> GetPendingSessions()
        {
            var sessions = await _trainerService.GetPendingSessionsByTrainerAsync(GetTrainerId());
            return Ok(sessions);
        }
    }
}
