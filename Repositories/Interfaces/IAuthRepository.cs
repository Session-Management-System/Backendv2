using Session_Management_System.Models;

namespace Session_Management_System.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<Role?> GetRoleByIdAsync(int roleId);
    }
}
