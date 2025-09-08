namespace Zenvestify.Web.Models
{
	public class IncomeTransaction
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid SourceId { get; set; }   // FK to OtherIncome.Id
		public DateTime TxnDate { get; set; }
		public decimal GrossAmount { get; set; }
		public decimal NetAmount { get; set; }
		public string? Notes { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
