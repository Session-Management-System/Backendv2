
namespace Session_Management_System.Models
{
    public class OTP
    {
        public int OtpId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string OTPCode { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}