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
            bool isValid = false;
            try
            {
                isValid = await _authService.VerifyOtpAsync(dto.Email, dto.otp);
                if (isValid)
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
                else
                {
                    return BadRequest("Invalid or expired OTP.");
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

        [HttpPost("Send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            try
            {
                var response = await _authService.GenerateAndSendOtpAsync(email);
                return Ok("Otp sent");
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword password)
        {
            bool isValid = false;
            isValid = await _authService.VerifyOtpAsync(password.Email, password.Otp);
            if (isValid)
            {
                if (await _authService.UpdatePasswordAsync(password.Email, password.NewPassword))
                    return Ok("Password Updated");
                else
                    return BadRequest("Password Not updated!!! Please Retry.");
            }
            else
            {
                return BadRequest("Invalid or expired OTP.");
            }

        }
    }
}
