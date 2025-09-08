using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Zenvestify.Web.Models;
using static Zenvestify.Shared.Models.UserProfileDtos;

namespace Zenvestify.Web.Data
{
    public class UserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        // ---------- helpers ----------
        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach(var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // ---------- Users ----------
        public Task<User?> GetUserByEmailAsync(string email)
            => _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<int> CreateAsync(User user)
        {
            if(user.Id == Guid.Empty) user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.isActive = true;
            _db.Users.Add(user);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> UpdatePasswordHashAsync(Guid userId, string newHash)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null) return 0;
            user.PasswordHash = newHash;
            user.UpdatedAt = DateTime.UtcNow;
            return await _db.SaveChangesAsync();
        }

        public Task<User?> GetUserByIdAsync(Guid id)
            => _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.isActive);

        // ---------- Password Reset (hashed) ----------
        public async Task<int> InsertPasswordResetTokenAsync(Guid userId, string token, DateTime expiresAt)
        {
            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = userId,
                TokenHash = HashToken(token),
                ExpiresAt = expiresAt,
                Used = false
            });
            return await _db.SaveChangesAsync();
        }

        public async Task<(Guid UserId, bool Valid)> ValidatePasswordResetTokenAsync(string token)
        {
            var th = HashToken(token);
            var row = await _db.PasswordResetTokens
                .Where(t => t.TokenHash == th && !t.Used && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            return row is null ? (Guid.Empty, false) : (row.UserId, true);
        }

        public async Task<int> MarkPasswordResetTokenUsedAsync(string token)
        {
            var th = HashToken(token);
            var row = await _db.PasswordResetTokens
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync(t => t.TokenHash == th && !t.Used);

            if(row == null) return 0;
            row.Used = true;
            return await _db.SaveChangesAsync();
        }

        // ---------- User Profile ----------
        public async Task<int> CreateUserProfileAsync(Guid userId)
        {
            if(!await _db.UserProfiles.AnyAsync(p => p.UserId == userId))
            {
                _db.UserProfiles.Add(new UserProfile
                {
                    UserId = userId,
                    Currency = "AUD",
                    Timezone = null,
                    OnboardingStatus = 0
                });
            }
            return await _db.SaveChangesAsync();
        }

        public Task<UserProfile?> GetUserProfileAsync(Guid userId)
            => _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        public async Task<int> UpdateOnboardingStatusAsync(Guid userId, int status)
        {
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if(profile == null)
            {
                _db.UserProfiles.Add(new UserProfile { UserId = userId, Currency = "AUD", OnboardingStatus = status });
            }
            else
            {
                profile.OnboardingStatus = status;
            }
            return await _db.SaveChangesAsync();
        }

        // ---------- Primary Income (IncomeSettings) ----------
        public async Task SetIncomeAsync(Guid userId, string payFrequency, decimal? net, decimal? gross, decimal? taxWithheld, int? usualPayDay)
        {
            var row = await _db.IncomeSettings.FirstOrDefaultAsync(x => x.UserId == userId);
            if(row == null)
            {
                row = new IncomeSettings
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PayFrequency = payFrequency,
                    NetPayPerCycle = net,
                    GrossPayPerCycle = gross,
                    TaxWithheld = taxWithheld,
                    UsualPayDay = usualPayDay,
                    OnboardingStage = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _db.IncomeSettings.Add(row);
            }
            else
            {
                row.PayFrequency = payFrequency;
                row.NetPayPerCycle = net;
                row.GrossPayPerCycle = gross;
                row.TaxWithheld = taxWithheld;
                row.UsualPayDay = usualPayDay;
                if(row.OnboardingStage == 0) row.OnboardingStage = 1;
                row.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public Task<IncomeSettings?> GetIncomeAsync(Guid userId)
            => _db.IncomeSettings.FirstOrDefaultAsync(x => x.UserId == userId);

        // ---------- Other Income ----------
        public async Task AddOtherIncomeAsync(Guid userId, string source, decimal amount, string frequency)
        {
            _db.OtherIncomes.Add(new OtherIncome
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Source = source,
                Amount = amount,
                Frequency = frequency,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<OtherIncome>> GetOtherIncomeAsync(Guid userId)
            => await _db.OtherIncomes
                        .Where(x => x.UserId == userId)
                        .OrderByDescending(x => x.CreatedAt)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task UpdateOtherIncomeAsync(Guid id, string source, decimal amount, string frequency)
        {
            var row = await _db.OtherIncomes.FirstOrDefaultAsync(x => x.Id == id);
            if(row == null) return;
            row.Source = source;
            row.Amount = amount;
            row.Frequency = frequency;
            row.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteOtherIncomeAsync(Guid id, Guid userId)
        {
            var row = await _db.OtherIncomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if(row == null) return;
            _db.OtherIncomes.Remove(row);
            await _db.SaveChangesAsync();
        }

        // ---------- Expenses ----------
        public async Task AddExpenseAsync(Guid userId, string category, decimal amount, DateTime dateSpent,
                                          string? notes, bool taxDeductible, bool workRelated)
        {
            _db.Expenses.Add(new Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = category,
                Amount = amount,
                DateSpent = dateSpent,
                Notes = notes,
                IsTaxDeductible = taxDeductible,
                IsWorkRelated = workRelated,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesAsync(Guid userId, DateTime? from = null, DateTime? to = null)
        {
            var q = _db.Expenses.Where(x => x.UserId == userId);
            if(from.HasValue && to.HasValue)
                q = q.Where(x => x.DateSpent >= from.Value && x.DateSpent <= to.Value);

            return await q.AsNoTracking().ToListAsync();
        }

        // ---------- Savings ----------
        public async Task AddSavingsGoalAsync(Guid userId, string name, decimal targetAmount, DateTime? targetDate, decimal amountSaved, int status)
        {
            _db.SavingsGoals.Add(new SavingsGoal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                TargetAmount = targetAmount,
                TargetDate = targetDate,
                AmountSavedToDate = amountSaved,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<SavingsGoal>> GetSavingsGoalsAsync(Guid userId)
            => await _db.SavingsGoals.Where(x => x.UserId == userId).AsNoTracking().ToListAsync();

        // ---------- Bills ----------
        public async Task AddBillAsync(Guid userId, string name, decimal amount, string frequency, DateTime firstDueDate)
        {
            _db.Bills.Add(new Bill
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Amount = amount,
                Frequency = frequency,
                FirstDueDate = firstDueDate,
                Status = 1
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Bill>> GetBillsAsync(Guid userId)
            => await _db.Bills.Where(x => x.UserId == userId).AsNoTracking().ToListAsync();

        // ---------- Loans ----------
        public async Task AddLoanAsync(Guid userId, LoanDto dto)
        {
            _db.Loans.Add(new LoanAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Principal = dto.Principal,
                InterestRate = dto.InterestRate,
                RepaymentAmount = dto.RepaymentAmount,
                RepaymentFrequency = dto.RepaymentFrequency,
                RemainingBalance = dto.RemainingBalance,
                RemainingTermMonths = dto.RemainingTermMonths,
                AmountPaidSoFar = dto.AmountPaidSoFar,
                StartDate = dto.StartDate
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<LoanAccount>> GetLoansAsync(Guid userId)
            => await _db.Loans.Where(x => x.UserId == userId).AsNoTracking().ToListAsync();

        // ---------- Tax ----------
        public async Task SetTaxSettingsAsync(Guid userId, decimal? taxWithheldPerCycle, bool medicareLevyExempt, bool privateHealthCover)
        {
            var row = await _db.TaxSettings.FirstOrDefaultAsync(x => x.UserId == userId);
            if(row == null)
            {
                _db.TaxSettings.Add(new TaxSettings
                {
                    UserId = userId,
                    TaxWithheldPerCycle = taxWithheldPerCycle,
                    MedicareLevyExempt = medicareLevyExempt,
                    PrivateHealthCover = privateHealthCover
                });
            }
            else
            {
                row.TaxWithheldPerCycle = taxWithheldPerCycle;
                row.MedicareLevyExempt = medicareLevyExempt;
                row.PrivateHealthCover = privateHealthCover;
            }
            await _db.SaveChangesAsync();
        }

        public Task<TaxSettings?> GetTaxSettingsAsync(Guid userId)
            => _db.TaxSettings.FirstOrDefaultAsync(x => x.UserId == userId);

        // ---------- Income Transactions ----------
        public async Task AddIncomeTransactionAsync(Guid userId, Guid sourceId, DateTime txnDate, decimal gross, decimal? net, string? notes)
        {
            _db.IncomeTransactions.Add(new IncomeTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SourceId = sourceId,
                TxnDate = txnDate,
                GrossAmount = gross,
                NetAmount = net ?? gross,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            });

            var settings = await _db.IncomeSettings.FirstOrDefaultAsync(x => x.UserId == userId);
            if(settings != null && settings.OnboardingStage < 2)
            {
                settings.OnboardingStage = 2;
                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<IncomeTransactionDto>> GetIncomeTransactionsAsync(Guid userId, Guid sourceId)
        {
            return await _db.IncomeTransactions
                .Where(t => t.UserId == userId && t.SourceId == sourceId)
                .OrderByDescending(t => t.TxnDate)
                .Select(t => new IncomeTransactionDto
                {
                    Id = t.Id,
                    SourceId = t.SourceId,
                    TxnDate = t.TxnDate,
                    GrossAmount = t.GrossAmount,
                    NetAmount = t.NetAmount,
                    Notes = t.Notes
                })
                .ToListAsync();
        }

        public async Task DeleteIncomeTransactionAsync(Guid userId, Guid txnId)
        {
            var row = await _db.IncomeTransactions.FirstOrDefaultAsync(t => t.Id == txnId && t.UserId == userId);
            if(row == null) return;
            _db.IncomeTransactions.Remove(row);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateIncomeTransactionAsync(Guid userId, Guid txnId, DateTime txnDate, decimal gross, decimal? net, string? notes)
        {
            var row = await _db.IncomeTransactions.FirstOrDefaultAsync(t => t.Id == txnId && t.UserId == userId);
            if(row == null) return;
            row.TxnDate = txnDate;
            row.GrossAmount = gross;
            row.NetAmount = net ?? gross;
            row.Notes = notes;
            row.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
