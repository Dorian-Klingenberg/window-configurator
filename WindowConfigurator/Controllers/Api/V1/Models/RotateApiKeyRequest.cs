namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class RotateApiKeyRequest
    {
        /// <summary>Optional expiry for the new key. Null means the key never expires.</summary>
        public DateTime? ExpiresAtUtc { get; set; }
    }
}
