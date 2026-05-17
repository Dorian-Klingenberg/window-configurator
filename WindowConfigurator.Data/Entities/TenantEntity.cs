namespace WindowConfigurator.Data.Entities
{
    public class TenantEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>The dealer or manufacturer's business name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>API key used by the tenant's CRM to authenticate with this platform.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>URL this platform POSTs completed quote payloads to.</summary>
        public string WebhookCallbackUrl { get; set; } = string.Empty;

        /// <summary>
        /// Keys of product lines this tenant is licensed to use.
        /// Example values: "energysaver-2500", "apex", "carriage"
        /// </summary>
        public List<string> AllowedProductLineKeys { get; set; } = new();

        /// <summary>
        /// When false, all windows in a quote session must share the same product line.
        /// When true, a session may contain windows from different product lines.
        /// </summary>
        public bool MixedProductLinesAllowed { get; set; } = false;

        public TenantBrandingConfig Branding { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
