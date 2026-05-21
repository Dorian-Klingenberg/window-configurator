using System.Globalization;
using System.Text.Json;
using WindowConfigurator.Data.Pricing;

namespace WindowConfigurator.Tests.Services;

public class PricingGridAlignerTests
{
    [Fact]
    public void AlignToCatalogTemplates_ExtendsPricingBreakpointsToCatalogMaximums()
    {
        var appDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData");
        var priceInfoPath = Path.Combine(appDataPath, "priceInfo.json");
        var root = JsonSerializer.Deserialize<PriceInfoRoot>(
            File.ReadAllText(priceInfoPath),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;

        PricingGridAligner.AlignToCatalogTemplates(root, appDataPath);

        var templates = LoadTemplateCatalogMaximums(appDataPath);
        var manufacturer = Assert.Single(root.PriceInfo.Manufacturers, m => m.Name == "All Weather Windows");

        foreach (var productLine in manufacturer.ProductLines)
        {
            Assert.True(templates.TryGetValue(productLine.Name, out var limits), $"Template limits not found for {productLine.Name}");

            foreach (var style in productLine.Styles)
            {
                if (!limits.StyleMax.TryGetValue(style.Name, out var styleMax))
                    continue;

                Assert.True(style.WidthBreakpoints[^1].Width >= styleMax.MaxWidth, $"{productLine.Name}/{style.Name} width grid not extended");
                var heightAtMaxWidth = style.WidthBreakpoints[^1].HeightBreakpoints[^1].Height;
                Assert.True(heightAtMaxWidth >= styleMax.MaxHeight, $"{productLine.Name}/{style.Name} height grid not extended");
            }

            foreach (var pane in productLine.PaneConfigurations)
            {
                Assert.True(pane.WidthBreakpoints[^1].Width >= limits.SectionMaxWidth, $"{productLine.Name}/{pane.Name} pane width grid not extended");
                Assert.True(pane.WidthBreakpoints[^1].HeightBreakpoints[^1].Height >= limits.SectionMaxHeight, $"{productLine.Name}/{pane.Name} pane height grid not extended");
            }

            foreach (var brickmould in productLine.BrickmouldStyles)
            {
                Assert.True(brickmould.WidthBreakpoints[^1].Width >= limits.BrickmouldMaxWidth, $"{productLine.Name}/{brickmould.Name} brickmould width grid not extended");
                Assert.True(brickmould.WidthBreakpoints[^1].HeightBreakpoints[^1].Height >= limits.BrickmouldMaxHeight, $"{productLine.Name}/{brickmould.Name} brickmould height grid not extended");
            }
        }
    }

    private static Dictionary<string, ProductLineTemplateLimits> LoadTemplateCatalogMaximums(string appDataPath)
    {
        var result = new Dictionary<string, ProductLineTemplateLimits>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(appDataPath, "*ItemTemplate.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var productLine = doc.RootElement.GetProperty("productLine");
            var name = productLine.GetProperty("name").GetString()!;

            var limits = new ProductLineTemplateLimits
            {
                FrameMaxWidth = ReadMeasurement(productLine.GetProperty("frameRestrictions").GetProperty("maxWidth")),
                FrameMaxHeight = ReadMeasurement(productLine.GetProperty("frameRestrictions").GetProperty("maxHeight"))
            };

            foreach (var style in productLine.GetProperty("operationalStyles").EnumerateArray())
            {
                var styleName = style.GetProperty("name").GetString()!;
                var restrictions = style.GetProperty("restrictions");
                var maxWidth = ReadMeasurement(restrictions.GetProperty("maxWidth"));
                var maxHeight = ReadMeasurement(restrictions.GetProperty("maxHeight"));
                limits.StyleMax[styleName] = (maxWidth, maxHeight);
                limits.SectionMaxWidth = Math.Max(limits.SectionMaxWidth, maxWidth);
                limits.SectionMaxHeight = Math.Max(limits.SectionMaxHeight, maxHeight);
            }

            var brickmouldMaxInset = 0m;
            foreach (var brickmould in productLine.GetProperty("brickmouldStyles").EnumerateArray())
            {
                brickmouldMaxInset = Math.Max(brickmouldMaxInset, ReadMeasurement(brickmould.GetProperty("width")));
            }

            limits.BrickmouldMaxWidth = limits.FrameMaxWidth + (brickmouldMaxInset * 2m);
            limits.BrickmouldMaxHeight = limits.FrameMaxHeight + (brickmouldMaxInset * 2m);

            result[name] = limits;
        }

        return result;
    }

    private static decimal ReadMeasurement(JsonElement measurement)
    {
        var sign = ReadDecimal(measurement.GetProperty("sign"));
        var whole = ReadDecimal(measurement.GetProperty("whole"));
        var numerator = ReadDecimal(measurement.GetProperty("numerator"));
        var denominator = ReadDecimal(measurement.GetProperty("denominator"));
        var magnitude = whole + (numerator / denominator);
        return sign < 0m ? -magnitude : magnitude;
    }

    private static decimal ReadDecimal(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Number
            ? element.GetDecimal()
            : decimal.Parse(element.GetString()!, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    private sealed class ProductLineTemplateLimits
    {
        public decimal FrameMaxWidth { get; set; }
        public decimal FrameMaxHeight { get; set; }
        public decimal SectionMaxWidth { get; set; }
        public decimal SectionMaxHeight { get; set; }
        public decimal BrickmouldMaxWidth { get; set; }
        public decimal BrickmouldMaxHeight { get; set; }
        public Dictionary<string, (decimal MaxWidth, decimal MaxHeight)> StyleMax { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
