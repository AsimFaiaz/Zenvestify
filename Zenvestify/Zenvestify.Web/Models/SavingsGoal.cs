namespace Zenvestify.Web.Models
{
	public class SavingsGoal
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Name { get; set; } = "";
		public decimal TargetAmount { get; set; }
		public DateTime? TargetDate { get; set; }
		public decimal AmountSavedToDate { get; set; }
		public int Status { get; set; } // 0=Active, 1=Completed
		public DateTime CreatedAt { get; set; }
	}
}
