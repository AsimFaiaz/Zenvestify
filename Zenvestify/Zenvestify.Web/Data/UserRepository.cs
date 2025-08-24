using Microsoft.Data.SqlClient;
using Dapper;
using Zenvestify.Web.Models;

namespace Zenvestify.Web.Data
{
	public class UserRepository
	{
		private readonly string _connectionString;
		public UserRepository(IConfiguration config)
		{
			_connectionString = config.GetConnectionString("DefaultConnection");
		}

		// get user by email
		public async Task<User?> GetUserByEmailAsync(string email)
		{
			using var conn = new SqlConnection(_connectionString);
			await conn.OpenAsync();
			var sql = "SELECT * FROM Users WHERE Email = @Email";
			return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
		}

		//insert new user
		public async Task<int> CreateAsync(User user)
		{
			using var conn = new SqlConnection(_connectionString);
			await conn.OpenAsync();
			var sql = @"INSERT INTO Users (FullName, Email, PasswordHash, CreatedAt, IsActive)
                        VALUES (@FullName, @Email, @PasswordHash, GETDATE(), 1)";
			return await conn.ExecuteAsync(sql, user);
		}

		//Forget and reset password
		public async Task<int> InsertPasswordResetTokenAsync(Guid userId, string token, DateTime expiresAt)
		{
			const string sql = @"INSERT INTO PasswordResetTokens (UserId, TOKEN, ExpiresAt, Used)
                         VALUES (@UserId, @Token, @ExpiresAt, 0)";
			using var conn = new SqlConnection(_connectionString);
			return await conn.ExecuteAsync(sql, new { UserId = userId, Token = token, ExpiresAt = expiresAt });
		}

		public async Task<(Guid UserId, bool Valid)> ValidatePasswordResetTokenAsync(string token)
		{
			const string sql = @"SELECT TOP 1 UserId
                         FROM PasswordResetTokens
                         WHERE TOKEN = @Token AND Used = 0 AND ExpiresAt > SYSUTCDATETIME()";
			using var conn = new SqlConnection(_connectionString);
			var userId = await conn.QueryFirstOrDefaultAsync<Guid?>(sql, new { Token = token });
			return (userId ?? Guid.Empty, userId.HasValue);
		}

		public async Task<int> MarkPasswordResetTokenUsedAsync(string token)
		{
			const string sql = @"UPDATE PasswordResetTokens SET Used = 1 WHERE TOKEN = @Token";
			using var conn = new SqlConnection(_connectionString);
			return await conn.ExecuteAsync(sql, new { Token = token });
		}

		public async Task<int> UpdatePasswordHashAsync(Guid userId, string newHash)
		{
			const string sql = @"UPDATE Users SET PasswordHash = @Hash, UpdateAt = SYSUTCDATETIME() WHERE Id = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.ExecuteAsync(sql, new { Hash = newHash, UserId = userId });
		}
	}
}