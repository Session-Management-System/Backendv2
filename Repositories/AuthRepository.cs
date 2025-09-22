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
        public async Task SaveOtpAsync(OTP otp)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO OTP (Email, OTPCode, ExpiryTime, IsUsed)
                             VALUES (@Email, @OTPCode, @ExpiryTime, @IsUsed)";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", otp.Email);
            cmd.Parameters.AddWithValue("@OTPCode", otp.OTPCode);
            cmd.Parameters.AddWithValue("@ExpiryTime", otp.ExpiryTime);
            cmd.Parameters.AddWithValue("@IsUsed", otp.IsUsed);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> otpInDb(string otp)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT COUNT(1) FROM OTP WHERE OTPCode = @otp";
            using SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.AddWithValue("@otp", otp);

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<OTP?> GetLatestOtpAsync(string email)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT TOP 1 * FROM OTP 
                             WHERE Email = @Email
                             ORDER BY OtpId DESC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new OTP
                {
                    OtpId = reader.GetInt32(reader.GetOrdinal("OtpId")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    OTPCode = reader.GetString(reader.GetOrdinal("OTPCode")),
                    ExpiryTime = reader.GetDateTime(reader.GetOrdinal("ExpiryTime")),
                    IsUsed = reader.GetBoolean(reader.GetOrdinal("IsUsed"))
                };
            }

            return null;
        }
        public async Task MarkOtpAsUsedAsync(int otpId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE OTP SET IsUsed = 1 WHERE OtpId = @OtpId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OtpId", otpId);

            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<bool> UpdatePasswordAsync(string email, string newPasswordHash)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Users SET PasswordHash = @Password WHERE Email = @EmailId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Password", newPasswordHash);
                    cmd.Parameters.AddWithValue("@EmailId", email);

                    await conn.OpenAsync();
                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }

        public async Task DeleteExpiredOtpsAsync()
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"DELETE FROM OTP WHERE ExpiryTime < GETUTCDATE() OR IsUsed = 1";

            using SqlCommand cmd = new SqlCommand(query, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
