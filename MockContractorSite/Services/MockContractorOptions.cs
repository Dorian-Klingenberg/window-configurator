namespace MockContractorSite.Services;

public sealed class MockContractorOptions
{
    public const string SectionName = "MockContractor";

    public string WindowConfiguratorBaseUrl { get; set; } = "http://localhost:5149";
    public Guid TenantId { get; set; } = new("00000000-0000-0000-0000-000000000002");
    public string ApiKey { get; set; } = "demo-api-key";
    public string DefaultProductLineKey { get; set; } = "energysaver-2500";
}
