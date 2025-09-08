namespace Zenvestify.Web.Models
{
	public class IncomeSettings
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string PayFrequency { get; set; } = "";
		public decimal? NetPayPerCycle { get; set; } 
		public decimal? GrossPayPerCycle { get; set; }
		public decimal? TaxWithheld { get; set; }
		public byte OnboardingStage { get; set; } = 0;   // 0=not done, 1=basic, 2=complete
		public int? UsualPayDay { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
