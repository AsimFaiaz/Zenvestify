namespace Zenvestify.Shared.Models
{
	public static class UserProfileDtos
	{
		public class IncomeDto
		{
			public string PayFrequency { get; set; } = "";
			public decimal? NetPayPerCycle { get; set; }
			public decimal? GrossPayPerCycle { get; set; }
			public decimal? TaxWithheld { get; set; }
		}

		public class OtherIncomeDto
		{
			public string Source { get; set; } = "";
			public decimal Amount { get; set; }
			public string Frequency { get; set; } = "";
		}

		public class SavingsGoalDto
		{
			public string Name { get; set; } = "";
			public decimal TargetAmount { get; set; }
			public DateTime? TargetDate { get; set; }
		}

		public class BillDto
		{
			public string Name { get; set; } = "";
			public decimal Amount { get; set; }
			public string Frequency { get; set; } = "";
			public DateTime FirstDueDate { get; set; }
		}

		public class LoanDto
		{
			public string Name { get; set; } = "";
			public decimal Principal { get; set; }
			public decimal InterestRate { get; set; }
			public decimal RepaymentAmount { get; set; }
			public string RepaymentFrequency { get; set; } = "";
			public decimal? RemainingBalance { get; set; }
			public int? RemainingTermMonths { get; set; }
			public decimal? AmountPaidSoFar { get; set; }
			public DateTime StartDate { get; set; }
		}

		public class TaxSettingsDto
		{
			public decimal? TaxWithheldPerCycle { get; set; }
			public bool MedicareLevyExempt { get; set; }
			public bool PrivateHealthCover { get; set; }
		}

		public class ExpenseDto
		{
			public string Category { get; set; } = "";
			public decimal Amount { get; set; }
			public DateTime DateSpent { get; set; }
			public string? Notes { get; set; }
			public bool IsTaxDeductible { get; set; }
			public bool IsWorkRelated { get; set; }
		}

		public class CompleteOnboardingDto
		{
			public string Currency { get; set; } = "AUD";
			public string? Timezone { get; set; }
		}
	}
}
