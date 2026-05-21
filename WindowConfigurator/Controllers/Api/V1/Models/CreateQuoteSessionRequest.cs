namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class CreateQuoteSessionRequest
    {
        public Guid TenantId { get; set; }
        public string? DefaultProductLineKey { get; set; }
        public string? ExternalReferenceId { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
