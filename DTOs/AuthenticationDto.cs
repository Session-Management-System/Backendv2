using System.ComponentModel.DataAnnotations;

namespace Session_Management_System.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be 8-20 characters long")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[@$!%*?&]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; } = 1;
    }

    public class LoginDto
    {

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

    }
    
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

    }
}