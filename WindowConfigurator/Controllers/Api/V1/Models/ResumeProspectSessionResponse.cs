namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class ResumeProspectSessionResponse
    {
        public Guid SessionId { get; set; }
        public string SessionUrl { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
