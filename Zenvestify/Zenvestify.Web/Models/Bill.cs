namespace Zenvestify.Web.Models
{
	public class Bill
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Name { get; set; } = "";
		public decimal Amount { get; set; }
		public string Frequency { get; set; } = "";
		public DateTime FirstDueDate { get; set; }
		public int Status { get; set; } // 1=Active, 0=Paused
	}
}
