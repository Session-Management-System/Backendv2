using System.ComponentModel.DataAnnotations;

namespace Session_Management_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        public string UserId { get; set; } = string.Empty;   // FK to Users.UserId
        [Required]
        public int SessionId { get; set; }   // FK to Sessions.SessionId
    }
}
