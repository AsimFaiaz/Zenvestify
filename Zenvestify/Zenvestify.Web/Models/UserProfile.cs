namespace Zenvestify.Web.Models
{
	public class UserProfile
	{
		public Guid UserId { get; set; }
		public string Currency { get; set; } = "AUD";
		public string? Timezone { get; set; }
		public int OnboardingStatus { get; set; } = 0;  //0 - Not started | 1 - In Prog | 2 - Completed
	}
}
