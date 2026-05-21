using System.Globalization;
using System.Text.Json;

namespace WindowConfigurator.Data.Pricing;

/// <summary>
/// Expands pricing breakpoint grids so they cover catalog-supported maximum dimensions.
/// This prevents valid catalog dimensions from falling off the edge of pricing tables.
/// </summary>
public static class PricingGridAligner
{
    public static void AlignToCatalogTemplates(PriceInfoRoot priceInfoRoot, string appDataPath)
    {
        var templates = LoadTemplateLimits(appDataPath);
        foreach (var manufacturer in priceInfoRoot.PriceInfo.Manufacturers)
        {
            foreach (var productLine in manufacturer.ProductLines)
            {
                if (!templates.TryGetValue(productLine.Name, out var limits))
                    continue;

                foreach (var style in productLine.Styles)
                {
                    if (!limits.StyleMax.TryGetValue(style.Name, out var styleMax))
                        continue;

                    ExtendGrid(style, styleMax.MaxWidth, styleMax.MaxHeight);
                }

                foreach (var grille in productLine.GrillePatterns)
                    ExtendGrid(grille, limits.SectionMaxWidth, limits.SectionMaxHeight);

                foreach (var sdl in productLine.SdlPatterns)
                    ExtendGrid(sdl, limits.SectionMaxWidth, limits.SectionMaxHeight);

                foreach (var pane in productLine.PaneConfigurations)
                    ExtendGrid(pane, limits.SectionMaxWidth, limits.SectionMaxHeight);

                foreach (var brickmould in productLine.BrickmouldStyles)
                    ExtendGrid(brickmould, limits.BrickmouldMaxWidth, limits.BrickmouldMaxHeight);
            }
        }
    }

    private static void ExtendGrid(BreakpointPricedItem item, decimal targetWidth, decimal targetHeight)
    {
        if (item.WidthBreakpoints.Count == 0)
            return;

        item.WidthBreakpoints = item.WidthBreakpoints
            .OrderBy(x => x.Width)
            .ToList();

        foreach (var width in item.WidthBreakpoints)
        {
            width.HeightBreakpoints = width.HeightBreakpoints
                .OrderBy(x => x.Height)
                .ToList();
            ExtendHeightBreakpoints(width.HeightBreakpoints, targetHeight);
        }

        var lastWidth = item.WidthBreakpoints[^1].Width;
        if (targetWidth <= lastWidth)
            return;

        var sourceWidth = item.WidthBreakpoints[^1];
        var previousWidth = item.WidthBreakpoints.Count > 1
            ? item.WidthBreakpoints[^2]
            : item.WidthBreakpoints[^1];

        var newWidth = new WidthBreakpoint { Width = targetWidth };
        foreach (var heightPoint in sourceWidth.HeightBreakpoints)
        {
            var sourcePpi = heightPoint.PricePerInch;
            var previousPpi = InterpolatePpiAtHeight(previousWidth.HeightBreakpoints, heightPoint.Height);
            var widthSpan = sourceWidth.Width - previousWidth.Width;
            var slope = widthSpan == 0m ? 0m : (sourcePpi - previousPpi) / widthSpan;
            var projectedPpi = sourcePpi + slope * (targetWidth - sourceWidth.Width);

            newWidth.HeightBreakpoints.Add(new HeightBreakpoint
            {
                Height = heightPoint.Height,
                PricePerInch = projectedPpi
            });
        }

        item.WidthBreakpoints.Add(newWidth);
    }

    private static void ExtendHeightBreakpoints(List<HeightBreakpoint> heights, decimal targetHeight)
    {
        if (heights.Count == 0)
            return;

        var lastHeight = heights[^1].Height;
        if (targetHeight <= lastHeight)
            return;

        var source = heights[^1];
        var previous = heights.Count > 1 ? heights[^2] : heights[^1];
        var heightSpan = source.Height - previous.Height;
        var slope = heightSpan == 0m ? 0m : (source.PricePerInch - previous.PricePerInch) / heightSpan;
        var projectedPpi = source.PricePerInch + slope * (targetHeight - source.Height);

        heights.Add(new HeightBreakpoint
        {
            Height = targetHeight,
            PricePerInch = projectedPpi
        });
    }

    private static decimal InterpolatePpiAtHeight(List<HeightBreakpoint> heights, decimal targetHeight)
    {
        if (heights.Count == 0)
            return 0m;

        if (targetHeight <= heights[0].Height)
            return heights[0].PricePerInch;

        for (var i = 1; i < heights.Count; i++)
        {
            var lower = heights[i - 1];
            var upper = heights[i];
            if (targetHeight <= upper.Height)
            {
                var span = upper.Height - lower.Height;
                if (span == 0m)
                    return lower.PricePerInch;

                var fraction = (targetHeight - lower.Height) / span;
                return lower.PricePerInch + fraction * (upper.PricePerInch - lower.PricePerInch);
            }
        }

        return heights[^1].PricePerInch;
    }

    private static Dictionary<string, ProductLineLimits> LoadTemplateLimits(string appDataPath)
    {
        var result = new Dictionary<string, ProductLineLimits>(StringComparer.OrdinalIgnoreCase);
        foreach (var templatePath in Directory.EnumerateFiles(appDataPath, "*ItemTemplate.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(templatePath));
            var root = doc.RootElement;
            if (!root.TryGetProperty("productLine", out var productLine))
                continue;

            var name = ReadString(productLine, "name");
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (!TryReadMeasurement(productLine, "frameRestrictions", "maxWidth", out var frameMaxWidth) ||
                !TryReadMeasurement(productLine, "frameRestrictions", "maxHeight", out var frameMaxHeight))
            {
                continue;
            }

            var limits = new ProductLineLimits
            {
                FrameMaxWidth = frameMaxWidth,
                FrameMaxHeight = frameMaxHeight,
                SectionMaxWidth = 0m,
                SectionMaxHeight = 0m,
                BrickmouldMaxInset = 0m
            };

            if (productLine.TryGetProperty("operationalStyles", out var styles) &&
                styles.ValueKind == JsonValueKind.Array)
            {
                foreach (var style in styles.EnumerateArray())
                {
                    var styleName = ReadString(style, "name");
                    if (string.IsNullOrWhiteSpace(styleName))
                        continue;

                    if (!TryReadMeasurement(style, "restrictions", "maxWidth", out var styleMaxWidth) ||
                        !TryReadMeasurement(style, "restrictions", "maxHeight", out var styleMaxHeight))
                    {
                        continue;
                    }

                    limits.StyleMax[styleName] = (styleMaxWidth, styleMaxHeight);
                    limits.SectionMaxWidth = Math.Max(limits.SectionMaxWidth, styleMaxWidth);
                    limits.SectionMaxHeight = Math.Max(limits.SectionMaxHeight, styleMaxHeight);
                }
            }

            if (productLine.TryGetProperty("brickmouldStyles", out var brickmouldStyles) &&
                brickmouldStyles.ValueKind == JsonValueKind.Array)
            {
                foreach (var brickmouldStyle in brickmouldStyles.EnumerateArray())
                {
                    if (TryReadMeasurement(brickmouldStyle, "width", out var inset))
                        limits.BrickmouldMaxInset = Math.Max(limits.BrickmouldMaxInset, inset);
                }
            }

            // Brickmould pricing uses top-level width/height, which includes brickmould extents.
            limits.BrickmouldMaxWidth = limits.FrameMaxWidth + (limits.BrickmouldMaxInset * 2m);
            limits.BrickmouldMaxHeight = limits.FrameMaxHeight + (limits.BrickmouldMaxInset * 2m);

            result[name] = limits;
        }

        return result;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static bool TryReadMeasurement(JsonElement parent, string propertyName, out decimal value)
    {
        value = 0m;
        if (!parent.TryGetProperty(propertyName, out var measurement))
            return false;

        return TryReadMeasurement(measurement, out value);
    }

    private static bool TryReadMeasurement(JsonElement parent, string parentProperty, string propertyName, out decimal value)
    {
        value = 0m;
        if (!parent.TryGetProperty(parentProperty, out var nested))
            return false;

        if (!nested.TryGetProperty(propertyName, out var measurement))
            return false;

        return TryReadMeasurement(measurement, out value);
    }

    private static bool TryReadMeasurement(JsonElement measurement, out decimal value)
    {
        value = 0m;
        if (measurement.ValueKind != JsonValueKind.Object)
            return false;

        if (!TryReadDecimal(measurement, "sign", out var sign) ||
            !TryReadDecimal(measurement, "whole", out var whole) ||
            !TryReadDecimal(measurement, "numerator", out var numerator) ||
            !TryReadDecimal(measurement, "denominator", out var denominator) ||
            denominator == 0m)
        {
            return false;
        }

        var magnitude = whole + (numerator / denominator);
        value = sign < 0m ? -magnitude : magnitude;
        return true;
    }

    private static bool TryReadDecimal(JsonElement element, string propertyName, out decimal value)
    {
        value = 0m;
        if (!element.TryGetProperty(propertyName, out var valueElement))
            return false;

        if (valueElement.ValueKind == JsonValueKind.Number)
            return valueElement.TryGetDecimal(out value);

        if (valueElement.ValueKind == JsonValueKind.String)
            return decimal.TryParse(valueElement.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

        return false;
    }

    private sealed class ProductLineLimits
    {
        public decimal FrameMaxWidth { get; set; }
        public decimal FrameMaxHeight { get; set; }
        public decimal SectionMaxWidth { get; set; }
        public decimal SectionMaxHeight { get; set; }
        public decimal BrickmouldMaxInset { get; set; }
        public decimal BrickmouldMaxWidth { get; set; }
        public decimal BrickmouldMaxHeight { get; set; }
        public Dictionary<string, (decimal MaxWidth, decimal MaxHeight)> StyleMax { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
