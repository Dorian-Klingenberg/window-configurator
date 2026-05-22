namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class TenantPolicySettingsRequest
    {
        public List<string> AllowedProductLineKeys { get; set; } = [];
        public bool MixedProductLinesAllowed { get; set; }
        public string? BrandingLogoUrl { get; set; }
        public string? BrandingPrimaryColor { get; set; }
        public string? BrandingAccentColor { get; set; }
    }
}
