namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class StartProspectSessionRequest
    {
        public Guid TenantId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
    }
}
