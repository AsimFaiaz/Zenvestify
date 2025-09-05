namespace Zenvestify.Web.Models
{
	public class IncomeSettings
	{
		public Guid UserId { get; set; }
		public string PayFrequency { get; set; } = "";
		public decimal? NetPayPerCycle { get; set; }   // ← make nullable
		public decimal? GrossPayPerCycle { get; set; }
		public decimal? TaxWithheld { get; set; }
	}
}
