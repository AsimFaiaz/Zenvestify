using Microsoft.EntityFrameworkCore;
using Zenvestify.Web.Models;

namespace Zenvestify.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<IncomeSettings> IncomeSettings => Set<IncomeSettings>();
        public DbSet<OtherIncome> OtherIncomes => Set<OtherIncome>();
        public DbSet<IncomeTransaction> IncomeTransactions => Set<IncomeTransaction>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
        public DbSet<Bill> Bills => Set<Bill>();
        public DbSet<LoanAccount> Loans => Set<LoanAccount>();
        public DbSet<TaxSettings> TaxSettings => Set<TaxSettings>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ---- Users (table: Users)
            mb.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);
                e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                e.Property(x => x.Email).HasMaxLength(320).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                e.Property(x => x.isActive).HasDefaultValue(true);
            });

            // ---- UserProfile (key = UserId) (table: UserProfile)
            mb.Entity<UserProfile>(e =>
            {
                e.ToTable("UserProfile");
                e.HasKey(x => x.UserId);
                e.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("AUD");
                e.Property(x => x.OnboardingStatus).HasDefaultValue(0);
            });

            // ---- IncomeSettings (table: IncomeSettings)
            mb.Entity<IncomeSettings>(e =>
            {
                e.ToTable("IncomeSettings");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.UserId).IsUnique();
                e.Property(x => x.NetPayPerCycle).HasPrecision(18, 2);
                e.Property(x => x.GrossPayPerCycle).HasPrecision(18, 2);
                e.Property(x => x.TaxWithheld).HasPrecision(18, 2);
                e.Property(x => x.OnboardingStage).HasDefaultValue((byte)0);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ---- OtherIncome (table: OtherIncome)
            mb.Entity<OtherIncome>(e =>
            {
                e.ToTable("OtherIncome");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.UserId);
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ---- Expenses (table: Expenses)
            mb.Entity<Expense>(e =>
            {
                e.ToTable("Expenses");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.UserId);
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ---- SavingsGoals (table: SavingsGoals)
            mb.Entity<SavingsGoal>(e =>
            {
                e.ToTable("SavingsGoals");
                e.HasKey(x => x.Id);
                e.Property(x => x.TargetAmount).HasPrecision(18, 2);
                e.Property(x => x.AmountSavedToDate).HasPrecision(18, 2);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ---- Bills (table: Bills)
            mb.Entity<Bill>(e =>
            {
                e.ToTable("Bills");
                e.HasKey(x => x.Id);
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.Property(x => x.Status).HasDefaultValue(1);
            });

            // ---- Loans (class LoanAccount -> table Loans)
            mb.Entity<LoanAccount>(e =>
            {
                e.ToTable("Loans");
                e.HasKey(x => x.Id);
                e.Property(x => x.Principal).HasPrecision(18, 2);
                e.Property(x => x.InterestRate).HasPrecision(9, 4);
                e.Property(x => x.RepaymentAmount).HasPrecision(18, 2);
            });

            // ---- TaxSettings (key = UserId) (table: TaxSettings)
            mb.Entity<TaxSettings>(e =>
            {
                e.ToTable("TaxSettings");
                e.HasKey(x => x.UserId);
                e.Property(x => x.TaxWithheldPerCycle).HasPrecision(18, 2);
            });

            // ---- IncomeTransactions (table: IncomeTransactions)
            mb.Entity<IncomeTransaction>(e =>
            {
                e.ToTable("IncomeTransactions");
                e.HasKey(x => x.Id);
                e.Property(x => x.GrossAmount).HasPrecision(18, 2);
                e.Property(x => x.NetAmount).HasPrecision(18, 2);
                e.HasIndex(x => new { x.UserId, x.SourceId, x.TxnDate });
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ---- PasswordResetTokens (secure: store TokenHash only)
            mb.Entity<PasswordResetToken>(e =>
            {
                e.ToTable("PasswordResetTokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
                e.HasIndex(x => new { x.TokenHash, x.Used });
                e.Property(x => x.Used).HasDefaultValue(false);
                e.Property(x => x.ExpiresAt).IsRequired();
            });
        }
    }
}
