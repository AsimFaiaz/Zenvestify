namespace Zenvestify.Web.Configs
{
	public class JwtOptions
	{
		public string Key { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public int ExpireMinutes { get; set; }
	}
}
