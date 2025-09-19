using Microsoft.AspNetCore.Mvc;
using Session_Management_System.Services.Interfaces;
using Session_Management_System.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Session_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
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
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
            var success = await _userService.CancelBookingAsync(bookingId, userId);
            if (!success) return NotFound(new { Message = "Booking not found or already cancelled" });
            return Ok(new { Message = "Booking cancelled successfully" });
        }

        // Get all bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
            var bookings = await _userService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        // Get upcoming bookings
        [HttpGet("bookings/upcoming")]
        public async Task<IActionResult> GetUpcomingBookings()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
            var bookings = await _userService.GetUpcomingBookingsAsync(userId);
            return Ok(bookings);
        }

        // Get completed sessions
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedSessions()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
            var sessions = await _userService.GetCompletedSessionsAsync(userId);
            return Ok(sessions);
        }

        // Get stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
            var stats = await _userService.GetUserStatsAsync(userId);
            return Ok(stats);
        }

        [HttpGet("available-sessions")]
        public async Task<ActionResult<IEnumerable<SessionResponseDto>>> GetAvailableSessions()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier));
            var sessions = await _userService.GetAvailableSessionsAsync(userId);
            return Ok(sessions);
        }
    }
}
