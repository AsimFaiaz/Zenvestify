namespace Zenvestify.Web.Models
{
	public class LoanAccount
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Name { get; set; } = "";
		public decimal Principal { get; set; }
		public decimal InterestRate { get; set; }
		public decimal RepaymentAmount { get; set; }
		public string RepaymentFrequency { get; set; } = "";
		public DateTime StartDate { get; set; }
		public int? RemainingTermMonths { get; set; }
		public decimal? RemainingBalance { get; set; }
		public decimal? AmountPaidSoFar { get; set; }
	}
}
