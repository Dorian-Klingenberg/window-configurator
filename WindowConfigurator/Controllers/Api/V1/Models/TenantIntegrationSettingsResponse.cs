namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class TenantIntegrationSettingsResponse
    {
        public Guid TenantId { get; set; }
        public string WebhookCallbackUrl { get; set; } = string.Empty;
    }
}
