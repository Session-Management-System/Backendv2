using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Session_Management_System.DTOs;
using Session_Management_System.Services.Interfaces;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;


namespace Session_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepo, IConfiguration config)
        {
            _authRepo = authRepo;
            _config = config;
        }

        // -------------------- REGISTER --------------------
        public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
        {
            if (await _authRepo.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already exists");

            var role = await _authRepo.GetRoleByIdAsync(dto.RoleId)
                       ?? throw new InvalidOperationException("Invalid role ID");

            string passwordHash = HashPassword(dto.Password);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                RoleId = role.RoleId,
            };

            user = await _authRepo.AddUserAsync(user);

            return GenerateJwtToken(user, role.RoleName);
        }

        // -------------------- LOGIN --------------------
        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetUserByEmailAsync(dto.Email);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials");

            var roleName = (await _authRepo.GetRoleByIdAsync(user.RoleId))?.RoleName ?? "User";

            return GenerateJwtToken(user, roleName);
        }

        // -------------------- PASSWORD HELPERS --------------------
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            string curr = HashPassword(password);
            return curr.Equals(storedHash);
        }

        // -------------------- JWT GENERATION --------------------
        private AuthResponse GenerateJwtToken(User user, string roleName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var keyString = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expires,
                UserName = $"{user.FirstName} {user.LastName}".Trim(),
                Role = roleName
            };
        }
    }
}
