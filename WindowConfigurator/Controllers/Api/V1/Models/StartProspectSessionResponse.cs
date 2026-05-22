namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class StartProspectSessionResponse
    {
        public Guid SessionId { get; set; }
        public string MagicLinkToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
