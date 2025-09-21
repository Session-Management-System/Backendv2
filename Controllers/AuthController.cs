using Session_Management_System.DTOs;
using Microsoft.AspNetCore.Mvc;
using Session_Management_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Session_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (dto.RoleId == 1 || dto.RoleId == 2)
                {
                    var response = await _authService.RegisterAsync(dto);
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Invalid Role Type");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
