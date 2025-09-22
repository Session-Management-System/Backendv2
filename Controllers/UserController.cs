using Microsoft.AspNetCore.Mvc;
using Session_Management_System.Services.Interfaces;
using Session_Management_System.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;

namespace Session_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Trainer,Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Book a session
        [HttpPost("book/{sessionId}")]
        public async Task<IActionResult> BookSession(int sessionId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            try
            {
                var bookingId = await _userService.BookSessionAsync(userId, sessionId);
                return Ok(new { BookingId = bookingId, Message = "Session booked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // Cancel a booking
        [HttpDelete("cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _userService.CancelBookingAsync(bookingId, userId);
            if (!success) return NotFound(new { Message = "Booking not found or already cancelled" });
            return Ok(new { Message = "Booking cancelled successfully" });
        }

        // Get all bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> GetUserBookings()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var bookings = await _userService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        // Get upcoming bookings
        [HttpGet("bookings/upcoming")]
        public async Task<IActionResult> GetUpcomingBookings()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var bookings = await _userService.GetUpcomingBookingsAsync(userId);
            return Ok(bookings);
        }

        // Get completed sessions
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedSessions()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var sessions = await _userService.GetCompletedSessionsAsync(userId);
            return Ok(sessions);
        }

        // Get stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var stats = await _userService.GetUserStatsAsync(userId);
            return Ok(stats);
        }

        [HttpGet("available-sessions")]
        public async Task<ActionResult<IEnumerable<SessionResponseDto>>> GetAvailableSessions()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var sessions = await _userService.GetAvailableSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpGet("details")]
        [Authorize]
        public async Task<ActionResult> GetUserDetails()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var details = await _userService.GetUserDetailsAsync(userId);
            return Ok(details);
        }

        [HttpPut("update-profile")]
        [Authorize(Roles = "User, Trainer")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto userdetails)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var updated = await _userService.UpdateUserProfileAsync(userId, userdetails);

            if (!updated)
                return BadRequest(new { message = "Failed to update profile." });

            return Ok(new {message= "Profile updated successfully."});
        }
    }
}
