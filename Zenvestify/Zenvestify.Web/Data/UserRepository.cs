using Dapper;
using Microsoft.Data.SqlClient;
using Zenvestify.Web.Models;
using static Zenvestify.Shared.Models.UserProfileDtos;

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

		//Create user profile - After login for first time
		public async Task<int> CreateUserProfileAsync(Guid userId)
		{
			const string sql = @"INSERT INTO UserProfile (UserId, Currency, Timezone, OnboardingStatus)
                         VALUES (@UserId, 'AUD', NULL, 0)";
			using var conn = new SqlConnection(_connectionString);
			return await conn.ExecuteAsync(sql, new { UserId = userId });
		}

		//Get user profile
		public async Task<UserProfile?> GetUserProfileAsync(Guid userId)
		{
			const string sql = "SELECT * FROM UserProfile WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryFirstOrDefaultAsync<UserProfile>(sql, new { UserId = userId });
		}

		//Onboarding status update
		public async Task<int> UpdateOnboardingStatusAsync(Guid userId, int status)
		{
			const string sql = @"UPDATE UserProfile SET OnboardingStatus = @Status WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.ExecuteAsync(sql, new { UserId = userId, Status = status });
		}

		//More than 1 income sources
		public async Task AddOtherIncomeAsync(Guid userId, string source, decimal amount, string frequency)
		{
			const string sql = @"INSERT INTO OtherIncome (Id, UserId, Source, Amount, Frequency, CreatedAt)
                     VALUES (NEWID(), @UserId, @Source, @Amount, @Frequency, SYSUTCDATETIME())";
			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { UserId = userId, Source = source, Amount = amount, Frequency = frequency });
		}

		public async Task<IEnumerable<dynamic>> GetOtherIncomeAsync(Guid userId)
		{
			const string sql = @"SELECT * FROM OtherIncome WHERE UserId=@UserId ORDER BY CreatedAt DESC";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryAsync(sql, new { UserId = userId });
		}

		public async Task UpdateOtherIncomeAsync(Guid id, string source, decimal amount, string frequency)
		{
			const string sql = @"
        UPDATE OtherIncome
        SET Source     = @Source,
            Amount     = @Amount,
            Frequency  = @Frequency,
            UpdatedAt  = SYSUTCDATETIME()
        WHERE Id = @Id";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { Id = id, Source = source, Amount = amount, Frequency = frequency });
		}

		public async Task DeleteOtherIncomeAsync(Guid id, Guid userId)
		{
			const string sql = @"DELETE FROM OtherIncome WHERE Id = @Id AND UserId = @UserId";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { Id = id, UserId = userId });
		}


		//Daily expenses tracking
		public async Task AddExpenseAsync(Guid userId, string category, decimal amount, DateTime dateSpent, string? notes, bool taxDeductible, bool workRelated)
		{
			const string sql = @"INSERT INTO Expenses (UserId, Category, Amount, DateSpent, Notes, IsTaxDeductible, IsWorkRelated) 
                         VALUES (@UserId, @Category, @Amount, @DateSpent, @Notes, @IsTaxDeductible, @IsWorkRelated)";
			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { UserId = userId, Category = category, Amount = amount, DateSpent = dateSpent, Notes = notes, IsTaxDeductible = taxDeductible, IsWorkRelated = workRelated });
		}

		public async Task<IEnumerable<dynamic>> GetExpensesAsync(Guid userId, DateTime? from = null, DateTime? to = null)
		{
			var sql = @"SELECT * FROM Expenses WHERE UserId=@UserId";
			if (from.HasValue && to.HasValue)
				sql += " AND DateSpent BETWEEN @From AND @To";

			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryAsync(sql, new { UserId = userId, From = from, To = to });
		}

		//Primary income
		public async Task SetIncomeAsync(Guid userId, string payFrequency, decimal? netPayPerCycle, decimal? grossPayPerCycle, decimal? taxWithheld, int? usualPayDay)
		{
			const string sql = @"
        MERGE IncomeSettings AS target
        USING (SELECT @UserId AS UserId) AS source
        ON target.UserId = source.UserId
        WHEN MATCHED THEN
            UPDATE SET PayFrequency = @PayFrequency,
                       NetPayPerCycle = @NetPayPerCycle,
                       GrossPayPerCycle = @GrossPayPerCycle,
                       TaxWithheld = @TaxWithheld,
                       UsualPayDay = @UsualPayDay,
                       OnboardingStage = CASE WHEN OnboardingStage = 0 THEN 1 ELSE OnboardingStage END,
                       UpdatedAt = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT (UserId, PayFrequency, NetPayPerCycle, GrossPayPerCycle, TaxWithheld, UsualPayDay, OnboardingStage, CreatedAt)
            VALUES (@UserId, @PayFrequency, @NetPayPerCycle, @GrossPayPerCycle, @TaxWithheld, @UsualPayDay, 1, SYSUTCDATETIME());";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { userId, payFrequency, netPayPerCycle, grossPayPerCycle, taxWithheld, usualPayDay });
		}


		public async Task<IncomeSettings?> GetIncomeAsync(Guid userId)
		{
			const string sql = "SELECT * FROM IncomeSettings WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryFirstOrDefaultAsync<IncomeSettings>(sql, new { userId });
		}

		//Savings
		public async Task AddSavingsGoalAsync(Guid userId, string name, decimal targetAmount, DateTime? targetDate, decimal amountSaved, int status)
		{
			const string sql = @"INSERT INTO SavingsGoals (UserId, Name, TargetAmount, TargetDate, AmountSavedToDate, Status)
                                 VALUES (@userId, @name, @targetAmount, @targetDate, @amountSaved, @status)";
			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { userId, name, targetAmount, targetDate, amountSaved, status });
		}

		public async Task<IEnumerable<SavingsGoal>> GetSavingsGoalsAsync(Guid userId)
		{
			const string sql = "SELECT * FROM SavingsGoals WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryAsync<SavingsGoal>(sql, new { userId });
		}

		//Bills
		public async Task AddBillAsync(Guid userId, string name, decimal amount, string frequency, DateTime firstDueDate)
		{
			const string sql = @"INSERT INTO Bills (UserId, Name, Amount, Frequency, FirstDueDate)
                                 VALUES (@userId, @name, @amount, @frequency, @firstDueDate)";
			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { userId, name, amount, frequency, firstDueDate });
		}

		public async Task<IEnumerable<Bill>> GetBillsAsync(Guid userId)
		{
			const string sql = "SELECT * FROM Bills WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryAsync<Bill>(sql, new { userId });
		}

		//Loans
		public async Task AddLoanAsync(Guid userId, LoanDto dto)
		{
			var sql = @"INSERT INTO Loans
                (UserId, Name, Principal, InterestRate, RepaymentAmount, RepaymentFrequency,
                 RemainingBalance, RemainingTermMonths, AmountPaidSoFar, StartDate)
                VALUES (@UserId, @Name, @Principal, @InterestRate, @RepaymentAmount,
                        @RepaymentFrequency, @RemainingBalance, @RemainingTermMonths,
                        @AmountPaidSoFar, @StartDate)";
			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new
			{
				UserId = userId,
				dto.Name,
				dto.Principal,
				dto.InterestRate,
				dto.RepaymentAmount,
				dto.RepaymentFrequency,
				dto.RemainingBalance,
				dto.RemainingTermMonths,
				dto.AmountPaidSoFar,
				dto.StartDate
			});
		}

		public async Task<IEnumerable<LoanAccount>> GetLoansAsync(Guid userId)
		{
			var sql = "SELECT * FROM Loans WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryAsync<LoanAccount>(sql, new { UserId = userId });
		}


		//Tax settings
		public async Task SetTaxSettingsAsync(Guid userId, decimal? taxWithheldPerCycle, bool medicareLevyExempt, bool privateHealthCover)
		{
			const string sql = @"
                MERGE TaxSettings AS target
                USING (SELECT @UserId AS UserId) AS source
                ON target.UserId = source.UserId
                WHEN MATCHED THEN
                    UPDATE SET TaxWithheldPerCycle = @taxWithheldPerCycle,
                               MedicareLevyExempt = @medicareLevyExempt,
                               PrivateHealthCover = @privateHealthCover
                WHEN NOT MATCHED THEN
                    INSERT (UserId, TaxWithheldPerCycle, MedicareLevyExempt, PrivateHealthCover)
                    VALUES (@UserId, @taxWithheldPerCycle, @medicareLevyExempt, @privateHealthCover);";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { userId, taxWithheldPerCycle, medicareLevyExempt, privateHealthCover });
		}

		public async Task<TaxSettings?> GetTaxSettingsAsync(Guid userId)
		{
			const string sql = "SELECT * FROM TaxSettings WHERE UserId = @UserId";
			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryFirstOrDefaultAsync<TaxSettings>(sql, new { userId });
		}

		//Income transactions

		public async Task AddIncomeTransactionAsync(Guid userId, Guid sourceId, DateTime txnDate, decimal gross, decimal? net, string? notes)
		{
			const string sql = @"
        INSERT INTO IncomeTransactions (Id, UserId, SourceId, TxnDate, GrossAmount, NetAmount, Notes, CreatedAt)
        VALUES (NEWID(), @UserId, @SourceId, @TxnDate, @GrossAmount, @NetAmount, @Notes, SYSUTCDATETIME());

        UPDATE IncomeSettings
        SET OnboardingStage = 2, UpdatedAt = SYSUTCDATETIME()
        WHERE UserId = @UserId AND OnboardingStage < 2;
    ";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { UserId = userId, SourceId = sourceId, TxnDate = txnDate, GrossAmount = gross, NetAmount = net ?? gross, Notes = notes });
		}



		public async Task<IEnumerable<IncomeTransactionDto>> GetIncomeTransactionsAsync(Guid userId, Guid sourceId)
		{
			const string sql = @"
        SELECT Id, SourceId, TxnDate, GrossAmount, NetAmount, Notes, CreatedAt
        FROM IncomeTransactions
        WHERE SourceId = @SourceId AND UserId = @UserId
        ORDER BY TxnDate DESC";

			using var conn = new SqlConnection(_connectionString);
			return await conn.QueryAsync<IncomeTransactionDto>(sql, new { SourceId = sourceId, UserId = userId });
		}


		public async Task DeleteIncomeTransactionAsync(Guid userId, Guid txnId)
		{
			const string sql = @"DELETE FROM IncomeTransactions WHERE Id = @Id AND UserId = @UserId";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { Id = txnId, UserId = userId });
		}

		public async Task UpdateIncomeTransactionAsync(Guid userId, Guid txnId, DateTime txnDate, decimal gross, decimal? net, string? notes)
		{
			const string sql = @"
        UPDATE IncomeTransactions
        SET TxnDate     = @TxnDate,
            GrossAmount = @GrossAmount,
            NetAmount   = @NetAmount,
            Notes       = @Notes,
            UpdatedAt   = SYSUTCDATETIME()
        WHERE Id = @Id AND UserId = @UserId";

			using var conn = new SqlConnection(_connectionString);
			await conn.ExecuteAsync(sql, new { Id = txnId, UserId = userId, TxnDate = txnDate, GrossAmount = gross, NetAmount = net ?? gross, Notes = notes });
		}


		//me
		public async Task<User?> GetUserByIdAsync(Guid id)
		{
			const string sql = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
			using var conn = new SqlConnection(_connectionString);
			await conn.OpenAsync();
			return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
		}
	}
}