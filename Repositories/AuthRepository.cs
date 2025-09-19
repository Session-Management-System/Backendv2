using Microsoft.Data.SqlClient;
using Session_Management_System.Models;
using Session_Management_System.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Session_Management_System.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> AddUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, RoleId) 
                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @RoleId); 
                    SELECT SCOPE_IDENTITY();", connection);

                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);

                user.UserId = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT TOP 1 * FROM Users WHERE Email = @Email", connection);
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            FirstName = reader["FirstName"].ToString() ?? "",
                            LastName = reader["LastName"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? "",
                            PasswordHash = reader["PasswordHash"].ToString() ?? "",
                            RoleId = Convert.ToInt32(reader["RoleId"])
                        };
                    }
                }
            }

            return null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Email = @Email", connection);
                command.Parameters.AddWithValue("@Email", email);

                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT RoleId, RoleName FROM Roles WHERE RoleId = @RoleId", connection);
                command.Parameters.AddWithValue("@RoleId", roleId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Role
                        {
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            RoleName = reader["RoleName"].ToString() ?? "User"
                        };
                    }
                }
            }

            return null;
        }
    }
}
