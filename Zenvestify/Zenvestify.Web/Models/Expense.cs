namespace Zenvestify.Web.Models
{
	public class Expense
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Category { get; set; } = "";
		public decimal Amount { get; set; }
		public DateTime DateSpent { get; set; }
		public string? Notes { get; set; }
		public bool IsTaxDeductible { get; set; }
		public bool IsWorkRelated { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
