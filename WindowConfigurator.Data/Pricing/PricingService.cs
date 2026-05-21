namespace WindowConfigurator.Data.Pricing;

/// <summary>
/// Computes server-authoritative prices using the same 2D interpolation logic as the
/// client-side pricing.js. Takes a pre-loaded PriceInfoRoot so there is no file I/O
/// inside the service — register as a singleton with the data loaded at startup.
/// </summary>
public class PricingService : IPricingService
{
    private readonly PriceInfoRoot _priceInfo;

    public PricingService(PriceInfoRoot priceInfo)
    {
        _priceInfo = priceInfo;
    }

    public decimal CalculatePrice(WindowPricingInput input)
    {
        var pl = FindProductLine(input.ManufacturerName, input.ProductLineName);

        decimal price = 0m;

        price += FrameColorPrice(input, pl);

        foreach (var section in input.Sections)
        {
            price += SectionPrice(section, pl);
            price += GrillePrice(section, pl);
            price += SdlPrice(section, pl);
            price += CrankPrice(section, pl);
            price += PaneConfigPrice(section, pl);
        }

        price += BrickmouldPrice(input, pl);

        // Adjustment and markup are percentage multipliers applied to the running total
        price += price * pl.Adjustment;
        price += price * pl.Markup;

        return Math.Round(price, 2);
    }

    // -------------------------------------------------------------------------
    // Per-component helpers
    // -------------------------------------------------------------------------

    private static decimal FrameColorPrice(WindowPricingInput input, ProductLinePricing pl)
    {
        var color = pl.FrameColors.FirstOrDefault(c => c.Name == input.FrameColorName);
        if (color is null || color.PricePerInch == 0m)
            return 0m;

        var width = input.OutsideWidthDecimal != 0m ? input.OutsideWidthDecimal : input.FrameWidthDecimal;
        var height = input.OutsideHeightDecimal != 0m ? input.OutsideHeightDecimal : input.FrameHeightDecimal;
        decimal perimeter = (width * 2) + (height * 2);
        return perimeter * color.PricePerInch;
    }

    private static decimal SectionPrice(SectionPricingInput section, ProductLinePricing pl)
    {
        var style = pl.Styles.FirstOrDefault(s => s.Name == section.StyleName);
        if (style is null)
            return 0m;

        var ppi = PriceInterpolator.Interpolate(style, section.WidthDecimal, section.HeightDecimal);
        return Perimeter(section) * ppi;
    }

    private static decimal GrillePrice(SectionPricingInput section, ProductLinePricing pl)
    {
        var grille = pl.GrillePatterns.FirstOrDefault(g => g.Name == section.GrillePatternName);
        if (grille is null)
            return 0m;

        var ppi = PriceInterpolator.Interpolate(grille, section.WidthDecimal, section.HeightDecimal);
        return Perimeter(section) * ppi;
    }

    private static decimal SdlPrice(SectionPricingInput section, ProductLinePricing pl)
    {
        var sdl = pl.SdlPatterns.FirstOrDefault(s => s.Name == section.SdlPatternName);
        if (sdl is null)
            return 0m;

        var ppi = PriceInterpolator.Interpolate(sdl, section.WidthDecimal, section.HeightDecimal);
        return Perimeter(section) * ppi;
    }

    private static decimal CrankPrice(SectionPricingInput section, ProductLinePricing pl)
    {
        var crank = pl.Cranks.FirstOrDefault(c => c.Name == section.CrankName);
        return crank?.Price ?? 0m;
    }

    private static decimal PaneConfigPrice(SectionPricingInput section, ProductLinePricing pl)
    {
        var config = pl.PaneConfigurations.FirstOrDefault(p => p.Name == section.PaneConfigurationName);
        if (config is null)
            return 0m;

        var ppi = PriceInterpolator.Interpolate(config, section.WidthDecimal, section.HeightDecimal);
        return Perimeter(section) * ppi;
    }

    private static decimal BrickmouldPrice(WindowPricingInput input, ProductLinePricing pl)
    {
        var brickmould = pl.BrickmouldStyles.FirstOrDefault(b => b.Name == input.BrickmouldStyleName);
        if (brickmould is null)
            return 0m;

        var ppi = InterpolateLegacyBrickmouldPricePerInch(brickmould, input.FrameWidthDecimal, input.FrameHeightDecimal);
        decimal framePerimeter = (input.FrameWidthDecimal * 2) + (input.FrameHeightDecimal * 2);
        return framePerimeter * ppi;
    }

    // -------------------------------------------------------------------------
    // Utilities
    // -------------------------------------------------------------------------

    private static decimal Perimeter(SectionPricingInput s) =>
        (s.WidthDecimal * 2) + (s.HeightDecimal * 2);

    /// <summary>
    /// Matches the legacy JavaScript brickmould calculator exactly.
    /// pricing.js computes the fully interpolated price-per-inch, but then returns the
    /// upper-height width-interpolated value instead. This quirk only affects brickmould.
    /// </summary>
    private static decimal InterpolateLegacyBrickmouldPricePerInch(BreakpointPricedItem item, decimal width, decimal height)
    {
        var widths = item.WidthBreakpoints;

        int upperWidthIdx = widths.FindIndex(w => w.Width >= width);
        int lowerWidthIdx;
        if (upperWidthIdx < 0)
        {
            upperWidthIdx = widths.Count - 1;
            lowerWidthIdx = upperWidthIdx;
        }
        else
        {
            lowerWidthIdx = upperWidthIdx > 0 ? upperWidthIdx - 1 : 0;
        }

        var lowerWidth = widths[lowerWidthIdx];
        var upperWidth = widths[upperWidthIdx];

        var lowerUpperHeight = FindUpperHeightBreakpoint(lowerWidth.HeightBreakpoints, height);
        var upperUpperHeight = FindUpperHeightBreakpoint(upperWidth.HeightBreakpoints, height);

        decimal widthRange = upperWidth.Width - lowerWidth.Width;
        decimal widthDistanceFromLower = width - lowerWidth.Width;
        decimal upperHeightWidthPpiDifference = widthRange <= 0m
            ? lowerUpperHeight.PricePerInch
            : (upperUpperHeight.PricePerInch - lowerUpperHeight.PricePerInch) / widthRange;

        return lowerUpperHeight.PricePerInch + (widthDistanceFromLower * upperHeightWidthPpiDifference);
    }

    private static HeightBreakpoint FindUpperHeightBreakpoint(List<HeightBreakpoint> heights, decimal height)
    {
        int upperIdx = heights.FindIndex(h => h.Height >= height);
        if (upperIdx < 0)
            upperIdx = heights.Count - 1;

        return heights[upperIdx];
    }

    private ProductLinePricing FindProductLine(string manufacturerName, string productLineName)
    {
        var manufacturer = _priceInfo.PriceInfo.Manufacturers
            .FirstOrDefault(m => m.Name == manufacturerName);

        if (manufacturer is null)
            throw new ArgumentException($"Manufacturer '{manufacturerName}' not found in price data.");

        var pl = manufacturer.ProductLines
            .FirstOrDefault(p => p.Name == productLineName);

        if (pl is null)
            throw new ArgumentException($"Product line '{productLineName}' not found for manufacturer '{manufacturerName}'.");

        return pl;
    }
}
