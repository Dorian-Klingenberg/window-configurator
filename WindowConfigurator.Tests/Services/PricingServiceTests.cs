using System.Text.Json;
using WindowConfigurator.Data.Pricing;
using Xunit;

namespace WindowConfigurator.Tests.Services;

public class PricingServiceTests
{
    // -------------------------------------------------------------------------
    // Deserialization
    // -------------------------------------------------------------------------

    [Fact]
    public void PriceInfoJson_Deserializes_ToTypedModel()
    {
        var json = File.ReadAllText(PriceInfoPath());
        var root = JsonSerializer.Deserialize<PriceInfoRoot>(json, JsonOptions());

        Assert.NotNull(root);
        Assert.NotNull(root.PriceInfo);
        Assert.NotEmpty(root.PriceInfo.Manufacturers);
    }

    [Fact]
    public void PriceInfoJson_ContainsAllThreeProductLines()
    {
        var root = LoadPriceInfo();
        var productLineNames = root.PriceInfo.Manufacturers
            .SelectMany(m => m.ProductLines)
            .Select(p => p.Name)
            .ToList();

        Assert.Contains("EnergySaver 2500", productLineNames);
        Assert.Contains("Apex", productLineNames);
        Assert.Contains("Carriage", productLineNames);
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasStylesWithBreakpoints()
    {
        var pl = GetProductLine("EnergySaver 2500");

        Assert.NotEmpty(pl.Styles);
        Assert.All(pl.Styles, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Name));
            Assert.NotEmpty(s.WidthBreakpoints);
            Assert.All(s.WidthBreakpoints, w => Assert.NotEmpty(w.HeightBreakpoints));
        });
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasFrameColors()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.FrameColors);
        Assert.All(pl.FrameColors, fc => Assert.False(string.IsNullOrWhiteSpace(fc.Name)));
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasCranks()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.Cranks);
        Assert.All(pl.Cranks, c => Assert.False(string.IsNullOrWhiteSpace(c.Name)));
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasBrickmouldStyles()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.BrickmouldStyles);
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasGrillePatterns()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.GrillePatterns);
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasSdlPatterns()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.SdlPatterns);
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasPaneConfigurations()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.PaneConfigurations);
    }

    [Fact]
    public void PriceInfoJson_EnergySaver_HasJambDepths()
    {
        var pl = GetProductLine("EnergySaver 2500");
        Assert.NotEmpty(pl.JambDepths);
    }

    // -------------------------------------------------------------------------
    // Interpolation — unit tests on the pure math
    // -------------------------------------------------------------------------

    [Fact]
    public void Interpolate_WhenDimensionsFallExactlyOnBreakpoint_ReturnsThatBreakpointPricePerInch()
    {
        // Arrange: 2x2 grid where the exact corner is requested
        var style = new BreakpointPricedItem
        {
            Name = "Test",
            WidthBreakpoints =
            [
                new WidthBreakpoint
                {
                    Width = 24m,
                    HeightBreakpoints = [new HeightBreakpoint { Height = 36m, PricePerInch = 2.0m }]
                },
                new WidthBreakpoint
                {
                    Width = 48m,
                    HeightBreakpoints = [new HeightBreakpoint { Height = 36m, PricePerInch = 1.0m }]
                }
            ]
        };

        var ppi = PriceInterpolator.Interpolate(style, width: 24m, height: 36m);

        Assert.Equal(2.0m, ppi, precision: 4);
    }

    [Fact]
    public void Interpolate_WhenWidthIsExactlyBetweenBreakpoints_InterpolatesCorrectly()
    {
        // width 36 is midway between 24 and 48; ppi should be midway between 2.0 and 1.0 = 1.5
        var style = new BreakpointPricedItem
        {
            Name = "Test",
            WidthBreakpoints =
            [
                new WidthBreakpoint
                {
                    Width = 24m,
                    HeightBreakpoints = [new HeightBreakpoint { Height = 36m, PricePerInch = 2.0m }]
                },
                new WidthBreakpoint
                {
                    Width = 48m,
                    HeightBreakpoints = [new HeightBreakpoint { Height = 36m, PricePerInch = 1.0m }]
                }
            ]
        };

        var ppi = PriceInterpolator.Interpolate(style, width: 36m, height: 36m);

        Assert.Equal(1.5m, ppi, precision: 4);
    }

    [Fact]
    public void Interpolate_WhenBothAxesInterpolate_Returns2DBilinearResult()
    {
        // 2x2 grid:
        //   (24,36)=4.0   (48,36)=2.0
        //   (24,72)=2.0   (48,72)=1.0
        // At (36, 54): midway on both axes
        //   lower height ppi at w=36 = midway(4.0, 2.0) = 3.0
        //   upper height ppi at w=36 = midway(2.0, 1.0) = 1.5
        //   final ppi at h=54 = midway(3.0, 1.5) = 2.25
        var style = new BreakpointPricedItem
        {
            Name = "Test",
            WidthBreakpoints =
            [
                new WidthBreakpoint
                {
                    Width = 24m,
                    HeightBreakpoints =
                    [
                        new HeightBreakpoint { Height = 36m, PricePerInch = 4.0m },
                        new HeightBreakpoint { Height = 72m, PricePerInch = 2.0m }
                    ]
                },
                new WidthBreakpoint
                {
                    Width = 48m,
                    HeightBreakpoints =
                    [
                        new HeightBreakpoint { Height = 36m, PricePerInch = 2.0m },
                        new HeightBreakpoint { Height = 72m, PricePerInch = 1.0m }
                    ]
                }
            ]
        };

        var ppi = PriceInterpolator.Interpolate(style, width: 36m, height: 54m);

        Assert.Equal(2.25m, ppi, precision: 4);
    }

    [Fact]
    public void Interpolate_WhenWidthExceedsAllBreakpoints_UsesLastBreakpoint()
    {
        var style = new BreakpointPricedItem
        {
            Name = "Test",
            WidthBreakpoints =
            [
                new WidthBreakpoint
                {
                    Width = 24m,
                    HeightBreakpoints = [new HeightBreakpoint { Height = 36m, PricePerInch = 2.0m }]
                },
                new WidthBreakpoint
                {
                    Width = 48m,
                    HeightBreakpoints = [new HeightBreakpoint { Height = 36m, PricePerInch = 1.0m }]
                }
            ]
        };

        var ppi = PriceInterpolator.Interpolate(style, width: 60m, height: 36m);

        Assert.Equal(1.0m, ppi, precision: 4);
    }

    // -------------------------------------------------------------------------
    // PricingService — end-to-end with real priceInfo.json data
    // -------------------------------------------------------------------------

    // Known values from priceInfo.json:
    //   EnergySaver 2500 / Casement at width=24, height=36 → ppi = 2.646
    //   White frame color → pricePerInch = 0.28
    //   Brickmould "None" → ppi = 0 at all breakpoints
    //   Grille "None", SDL "None", Crank "None", PaneConfig "Dual" → all 0
    //   Adjustment = 0, Markup = 0
    //
    //   Frame color contribution: 0.28 × (24×2 + 36×2) = 0.28 × 120 = 33.6
    //   Section contribution:     2.646 × 120            = 317.52
    //   Total expected:           351.12

    [Fact]
    public void CalculatePrice_SingleCasementAtExactBreakpoints_ReturnsCorrectTotal()
    {
        var service = new PricingService(LoadPriceInfo());

        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = "EnergySaver 2500",
            FrameWidthDecimal = 24m,
            FrameHeightDecimal = 36m,
            FrameColorName = "White",
            BrickmouldStyleName = "None",
            Sections =
            [
                new SectionPricingInput
                {
                    WidthDecimal = 24m,
                    HeightDecimal = 36m,
                    StyleName = "Casement",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "None",
                    PaneConfigurationName = "Dual"
                }
            ]
        };

        var price = service.CalculatePrice(input);

        Assert.Equal(351.12m, price, precision: 2);
    }

    [Fact]
    public void CalculatePrice_CrankWithPrice_AddsCrankToTotal()
    {
        // Encore/ADA crank costs $37.99 flat
        var service = new PricingService(LoadPriceInfo());

        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = "EnergySaver 2500",
            FrameWidthDecimal = 24m,
            FrameHeightDecimal = 36m,
            FrameColorName = "White",
            BrickmouldStyleName = "None",
            Sections =
            [
                new SectionPricingInput
                {
                    WidthDecimal = 24m,
                    HeightDecimal = 36m,
                    StyleName = "Casement",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "Encore/ADA",
                    PaneConfigurationName = "Dual"
                }
            ]
        };

        var price = service.CalculatePrice(input);

        Assert.Equal(351.12m + 37.99m, price, precision: 2);
    }

    [Fact]
    public void CalculatePrice_InterpolatedBrickmouldConfiguration_MatchesLegacyJavaScript()
    {
        var service = new PricingService(LoadPriceInfo());

        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = "EnergySaver 2500",
            FrameWidthDecimal = 77m,
            FrameHeightDecimal = 43.375m,
            OutsideWidthDecimal = 77m,
            OutsideHeightDecimal = 43.375m,
            FrameColorName = "White",
            BrickmouldStyleName = "2 Inch",
            Sections =
            [
                new SectionPricingInput
                {
                    WidthDecimal = 27.375m,
                    HeightDecimal = 44.0625m,
                    StyleName = "Picture",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "None",
                    PaneConfigurationName = "Dual"
                },
                new SectionPricingInput
                {
                    WidthDecimal = 48.4375m,
                    HeightDecimal = 44.0625m,
                    StyleName = "Picture",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "None",
                    PaneConfigurationName = "Dual"
                }
            ]
        };

        var price = service.CalculatePrice(input);

        Assert.Equal(1066.75m, price, precision: 2);
    }

    [Fact]
    public void CalculatePrice_BrickmouldFallsBackToFrameMeasurements_WhenExplicitBrickmouldDimensionsAreAbsent()
    {
        var service = new PricingService(LoadPriceInfo());

        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = "EnergySaver 2500",
            FrameWidthDecimal = 37m,
            FrameHeightDecimal = 49m,
            OutsideWidthDecimal = 36m,
            OutsideHeightDecimal = 48m,
            FrameColorName = "White",
            BrickmouldStyleName = "2 Inch",
            Sections =
            [
                new SectionPricingInput
                {
                    WidthDecimal = 36m,
                    HeightDecimal = 48m,
                    StyleName = "Picture",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "None",
                    PaneConfigurationName = "Dual"
                }
            ]
        };

        var price = service.CalculatePrice(input);

        Assert.Equal(312.62m, price, precision: 2);
    }

    [Fact]
    public void CalculatePrice_BrickmouldUsesExplicitBrickmouldPricingMeasurements()
    {
        var service = new PricingService(LoadPriceInfo());

        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = "EnergySaver 2500",
            FrameWidthDecimal = 66.5625m,
            FrameHeightDecimal = 42.375m,
            BrickmouldPricingWidthDecimal = 46m,
            BrickmouldPricingHeightDecimal = 46.0625m,
            OutsideWidthDecimal = 45m,
            OutsideHeightDecimal = 43.375m,
            FrameColorName = "White",
            BrickmouldStyleName = "2 Inch",
            Sections =
            [
                new SectionPricingInput
                {
                    WidthDecimal = 22.1875m,
                    HeightDecimal = 44.0625m,
                    StyleName = "Picture",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "None",
                    PaneConfigurationName = "Dual"
                },
                new SectionPricingInput
                {
                    WidthDecimal = 22.1875m,
                    HeightDecimal = 44.0625m,
                    StyleName = "Picture",
                    GrillePatternName = "None",
                    SdlPatternName = "None",
                    CrankName = "None",
                    PaneConfigurationName = "Dual"
                }
            ]
        };

        var price = service.CalculatePrice(input);

        Assert.Equal(447.75m, price, precision: 2);
    }

    [Fact]
    public void CalculatePrice_UnknownProductLine_ThrowsArgumentException()
    {
        var service = new PricingService(LoadPriceInfo());

        var input = new WindowPricingInput
        {
            ManufacturerName = "All Weather Windows",
            ProductLineName = "NonExistent Line",
            FrameWidthDecimal = 24m,
            FrameHeightDecimal = 36m,
            FrameColorName = "White",
            BrickmouldStyleName = "None",
            Sections = []
        };

        Assert.Throws<ArgumentException>(() => service.CalculatePrice(input));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string PriceInfoPath() =>
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", "priceInfo.json");

    private static JsonSerializerOptions JsonOptions() =>
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static PriceInfoRoot LoadPriceInfo()
    {
        var json = File.ReadAllText(PriceInfoPath());
        return JsonSerializer.Deserialize<PriceInfoRoot>(json, JsonOptions())!;
    }

    private static ProductLinePricing GetProductLine(string name)
    {
        var root = LoadPriceInfo();
        return root.PriceInfo.Manufacturers
            .SelectMany(m => m.ProductLines)
            .First(p => p.Name == name);
    }
}
