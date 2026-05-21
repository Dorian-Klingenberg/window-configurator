namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class QuoteSessionResponse
    {
        public Guid SessionId { get; set; }
        public Guid TenantId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DefaultProductLineKey { get; set; }
        public string? ExternalReferenceId { get; set; }
        public string? CustomerEmail { get; set; }
        public string SessionUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<QuoteSessionItemResponse> Items { get; set; } = [];
    }
}
