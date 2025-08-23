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
	}
}