namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class RotateApiKeyResponse
    {
        public Guid TenantId { get; set; }

        /// <summary>The new API key value. This is the only time the raw key is returned.</summary>
        public string ApiKey { get; set; } = string.Empty;

        public DateTime IssuedAtUtc { get; set; }

        /// <summary>Null means the key never expires.</summary>
        public DateTime? ExpiresAtUtc { get; set; }
    }
}
