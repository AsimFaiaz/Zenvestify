namespace Zenvestify.Web.Models
{
	public class OtherIncome
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Source { get; set; } = "";
		public decimal Amount { get; set; }
		public string Frequency { get; set; } = "";
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
