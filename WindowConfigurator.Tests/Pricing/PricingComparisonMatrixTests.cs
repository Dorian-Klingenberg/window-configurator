using System.Text.Json;
using System.Text.Json.Serialization;
using WindowConfigurator.Data.Pricing;
using Xunit;

namespace WindowConfigurator.Tests.Pricing;

public class PricingComparisonMatrixTests
{
    private readonly IPricingService _pricingService = new PricingService(LoadPriceInfo());

    [Fact]
    public void GeneratedMatrix_InRangeCases_MatchLegacyJavaScript()
    {
        var fixture = LoadFixture();
        var mismatches = new List<string>();

        foreach (var @case in fixture.Cases.Where(c => !c.Past))
        {
            var actual = _pricingService.CalculatePrice(ToInput(@case));
            if (actual != @case.Expected)
            {
                mismatches.Add($"{@case.Id} expected {@case.Expected:F2} actual {actual:F2}");
                if (mismatches.Count >= 25)
                    break;
            }
        }

        Assert.True(mismatches.Count == 0,
            "In-range pricing mismatches found:\n" + string.Join("\n", mismatches));
    }

    [Fact]
    public void GeneratedMatrix_IncludesSubmittedBrickmouldSizingRegression()
    {
        var fixture = LoadFixture();

        Assert.Contains(fixture.Cases, c =>
            c.Id == "EnergySaver 2500|submitted-regression|two-section|brickmould-sizing-dimensions" &&
            c.BrickmouldPricingWidth == 46m &&
            c.BrickmouldPricingHeight == 46.0625m &&
            c.FrameWidth == 66.5625m &&
            c.FrameHeight == 42.375m &&
            c.Expected == 447.75m);
    }

    [Fact]
    public void GeneratedMatrix_PastFinalBreakpointCases_ShowParityGaps()
    {
        var fixture = LoadFixture();
        var mismatches = new List<string>();
        var checkedCount = 0;

        foreach (var @case in fixture.Cases.Where(c => c.Past))
        {
            checkedCount++;
            var actual = _pricingService.CalculatePrice(ToInput(@case));
            if (actual != @case.Expected)
            {
                mismatches.Add($"{@case.Id} expected {@case.Expected:F2} actual {actual:F2}");
                if (mismatches.Count >= 25)
                    break;
            }
        }

        Assert.True(checkedCount > 0, "Expected at least one past-final-breakpoint case in the generated fixture.");
        Assert.True(mismatches.Count > 0,
            "Expected at least one parity gap past final breakpoints, but none were found.");
    }

    private static WindowPricingInput ToInput(PricingComparisonCase @case)
    {
        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = @case.ProductLine,
            FrameWidthDecimal = @case.FrameWidth,
            FrameHeightDecimal = @case.FrameHeight,
            BrickmouldPricingWidthDecimal = @case.BrickmouldPricingWidth == 0m
                ? @case.FrameWidth
                : @case.BrickmouldPricingWidth,
            BrickmouldPricingHeightDecimal = @case.BrickmouldPricingHeight == 0m
                ? @case.FrameHeight
                : @case.BrickmouldPricingHeight,
            OutsideWidthDecimal = @case.OutsideWidth,
            OutsideHeightDecimal = @case.OutsideHeight,
            FrameColorName = @case.FrameColor,
            BrickmouldStyleName = @case.BrickmouldStyle
        };

        foreach (var section in @case.Sections)
        {
            input.Sections.Add(new SectionPricingInput
            {
                StyleName = section.StyleName,
                CrankName = section.CrankName,
                GrillePatternName = section.GrillePatternName,
                SdlPatternName = section.SdlPatternName,
                PaneConfigurationName = section.PaneConfigurationName,
                WidthDecimal = section.Width,
                HeightDecimal = section.Height
            });
        }

        return input;
    }

    private static PricingComparisonFixture LoadFixture()
    {
        var path = Path.Combine(FindSolutionRoot(), "WindowConfigurator.Tests", "Pricing", "pricing-comparison-fixture.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PricingComparisonFixture>(json)!;
    }

    private static PriceInfoRoot LoadPriceInfo()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", "priceInfo.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PriceInfoRoot>(
            json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }

    private static string FindSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "WindowConfigurator.sln")))
                return current.FullName;

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    public sealed class PricingComparisonFixture
    {
        [JsonPropertyName("meta")]
        public PricingComparisonMetadata Meta { get; set; } = new();

        [JsonPropertyName("cases")]
        public List<PricingComparisonCase> Cases { get; set; } = [];
    }

    public sealed class PricingComparisonMetadata
    {
        [JsonPropertyName("generatedAtUtc")]
        public string GeneratedAtUtc { get; set; } = string.Empty;

        [JsonPropertyName("totalCases")]
        public int TotalCases { get; set; }

        [JsonPropertyName("inRangeCases")]
        public int InRangeCases { get; set; }

        [JsonPropertyName("pastFinalBreakpointCases")]
        public int PastFinalBreakpointCases { get; set; }
    }

    public sealed class PricingComparisonCase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("pl")]
        public string ProductLine { get; set; } = string.Empty;

        [JsonPropertyName("sc")]
        public int SectionCount { get; set; }

        [JsonPropertyName("past")]
        public bool Past { get; set; }

        [JsonPropertyName("fw")]
        public decimal FrameWidth { get; set; }

        [JsonPropertyName("fh")]
        public decimal FrameHeight { get; set; }

        [JsonPropertyName("bw")]
        public decimal BrickmouldPricingWidth { get; set; }

        [JsonPropertyName("bh")]
        public decimal BrickmouldPricingHeight { get; set; }

        [JsonPropertyName("ow")]
        public decimal OutsideWidth { get; set; }

        [JsonPropertyName("oh")]
        public decimal OutsideHeight { get; set; }

        [JsonPropertyName("fc")]
        public string FrameColor { get; set; } = string.Empty;

        [JsonPropertyName("bm")]
        public string BrickmouldStyle { get; set; } = string.Empty;

        [JsonPropertyName("pn")]
        public string PaneConfiguration { get; set; } = string.Empty;

        [JsonPropertyName("s")]
        public List<PricingComparisonSection> Sections { get; set; } = [];

        [JsonPropertyName("e")]
        public decimal Expected { get; set; }
    }

    public sealed class PricingComparisonSection
    {
        [JsonPropertyName("st")]
        public string StyleName { get; set; } = string.Empty;

        [JsonPropertyName("cr")]
        public string CrankName { get; set; } = string.Empty;

        [JsonPropertyName("gr")]
        public string GrillePatternName { get; set; } = string.Empty;

        [JsonPropertyName("sd")]
        public string SdlPatternName { get; set; } = string.Empty;

        [JsonPropertyName("pn")]
        public string PaneConfigurationName { get; set; } = string.Empty;

        [JsonPropertyName("w")]
        public decimal Width { get; set; }

        [JsonPropertyName("h")]
        public decimal Height { get; set; }
    }
}
