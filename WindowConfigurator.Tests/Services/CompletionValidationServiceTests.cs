using System.Text.Json;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Pricing;
using WindowConfigurator.Data.Validation;

namespace WindowConfigurator.Tests.Services;

public class CompletionValidationServiceTests
{
    private readonly CompletionValidationService _validator = new(LoadPriceInfo());
    private readonly string _energySaverTemplate = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", "energySaverItemTemplate.json"));

    [Fact]
    public void Validate_WhenPaneConfigurationExceedsPricingBreakpoint_ReturnsError()
    {
        var payload = BuildPayload(
            frameWidth: 72.5m,
            sectionWidth: 72.5m,
            styleName: "Picture");

        var result = _validator.Validate(
            payload,
            "energysaver-2500",
            UnrestrictedTenant(),
            _energySaverTemplate);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Field.Contains("paneConfiguration"));
    }

    [Fact]
    public void Validate_WhenBrickmouldExceedsPricingBreakpoint_ReturnsError()
    {
        var payload = BuildPayload(
            frameWidth: 73.5m,
            sectionWidth: 72m,
            styleName: "Picture",
            brickmouldStyleName: "2 Inch");

        var result = _validator.Validate(
            payload,
            "energysaver-2500",
            UnrestrictedTenant(),
            _energySaverTemplate);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Field == "brickmouldStyle");
    }

    private static TenantEntity UnrestrictedTenant()
        => new()
        {
            Name = "Test Dealer",
            ApiKey = "test-api-key"
        };

    private static JsonElement BuildPayload(
        decimal frameWidth = 36m,
        decimal frameHeight = 48m,
        decimal sectionWidth = 36m,
        decimal sectionHeight = 48m,
        string styleName = "Picture",
        string brickmouldStyleName = "None")
    {
        return JsonSerializer.SerializeToElement(new
        {
            productLine = new
            {
                key = "energysaver-2500",
                name = "EnergySaver 2500",
                manufacturerName = "All Weather Windows"
            },
            frameWidth = Measurement(frameWidth),
            frameHeight = Measurement(frameHeight),
            osmWidth = Measurement(frameWidth),
            osmHeight = Measurement(frameHeight),
            frameColor = new { name = "White" },
            brickmouldStyle = new { name = brickmouldStyleName },
            paneConfiguration = new { name = "Dual" },
            sections = new[]
            {
                new
                {
                    width = Measurement(sectionWidth),
                    height = Measurement(sectionHeight),
                    style = new { name = styleName },
                    grillePattern = new { name = "None" },
                    sdlPattern = new { name = "None" },
                    crank = new { name = "None" }
                }
            }
        });
    }

    private static object Measurement(decimal value)
    {
        var sixteenths = (int)Math.Round(value * 16m);
        return new
        {
            sign = sixteenths < 0 ? -1 : 1,
            whole = Math.Abs(sixteenths) / 16,
            numerator = Math.Abs(sixteenths) % 16,
            denominator = 16
        };
    }

    private static PriceInfoRoot LoadPriceInfo()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", "priceInfo.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PriceInfoRoot>(
            json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }
}
