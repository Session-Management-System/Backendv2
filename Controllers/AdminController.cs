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
            var success = await _service.RejectSessionAsync(id);
            return success ? Ok(new { Message = "Session rejected and deleted." }) : NotFound("Session not found.");
        }

        [HttpGet("Active-user-trainers")]
        public async Task<IActionResult> UserCountStats()
        {
            var count = await _service.UserCountStats();
            return Ok(count);
        }

        // [HttpPost("add-trainer")]
        // public async Task<IActionResult> AddTrainer([FromBody] AddTrainerDto dto)
        // {
        //     var success = await _service.AddTrainerAsync(dto);
        //     return success ? Ok(new { Message = "Trainer added." }) : BadRequest("Failed to add trainer.");
        // }

        // [HttpPost("promote-to-trainer/{userId}")]
        // public async Task<IActionResult> PromoteUserToTrainer(int userId)
        // {
        //     var success = await _service.PromoteUserToTrainerAsync(userId);
        //     return success ? Ok(new { Message = "User promoted to Trainer." }) : NotFound("User not found.");
        // }

        // [HttpDelete("delete-trainer/{id}")]
        // public async Task<IActionResult> DeleteTrainer(int id)
        // {
        //     var success = await _service.DeleteTrainerAsync(id);
        //     return success ? Ok(new { Message = "Trainer deleted." }) : NotFound("Trainer not found.");
        // }
    }
}
