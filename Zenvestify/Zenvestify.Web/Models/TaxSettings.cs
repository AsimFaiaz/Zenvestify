namespace Zenvestify.Web.Models
{
	public class TaxSettings
	{
		public Guid UserId { get; set; }
		public decimal? TaxWithheldPerCycle { get; set; }
		public bool MedicareLevyExempt { get; set; }
		public bool PrivateHealthCover { get; set; }
	}
}
