namespace Zenvestify.Web.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
    }
}
