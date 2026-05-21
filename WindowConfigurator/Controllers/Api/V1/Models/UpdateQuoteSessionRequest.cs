namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class UpdateQuoteSessionRequest
    {
        public string? DefaultProductLineKey { get; set; }
        public string? ExternalReferenceId { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
