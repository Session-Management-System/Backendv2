using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Session_Management_System.DTOs;
using Session_Management_System.Services.Interfaces;

namespace Session_Management_System.Controllers
{
    [Authorize(Roles = "User,Trainer,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _service;

        public SessionController(ISessionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSessions() =>
            Ok(await _service.GetAllSessionsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var session = await _service.GetSessionByIdAsync(id);
            if (session == null) return NotFound(new { Message = "Session not found" });
            return Ok(session);
        }

        [Authorize(Roles = "Trainer,Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateSession([FromBody] SessionUpdateDto dto) =>
            Ok(new { Message = await _service.UpdateSessionAsync(dto) });

        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveSession(int id, [FromQuery] bool approve = true) =>
            Ok(new { Message = await _service.ApproveSessionAsync(id, approve) });

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id) =>
            Ok(new { Message = await _service.DeleteSessionAsync(id) });
    }
}
