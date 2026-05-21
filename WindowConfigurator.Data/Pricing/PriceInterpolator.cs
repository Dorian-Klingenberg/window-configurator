namespace WindowConfigurator.Data.Pricing;

/// <summary>
/// Performs bilinear interpolation over a 2D width/height breakpoint grid.
/// Returns the interpolated price-per-inch for the given dimensions.
/// </summary>
public static class PriceInterpolator
{
    public static decimal Interpolate(BreakpointPricedItem item, decimal width, decimal height)
    {
        var widths = item.WidthBreakpoints;

        // Find the two bounding width breakpoints; clamp when width exceeds all breakpoints
        int upperWidthIdx = widths.FindIndex(w => w.Width >= width);
        int lowerWidthIdx;
        if (upperWidthIdx < 0)
        {
            upperWidthIdx = widths.Count - 1;
            lowerWidthIdx = upperWidthIdx; // clamp — no extrapolation
        }
        else
        {
            lowerWidthIdx = upperWidthIdx > 0 ? upperWidthIdx - 1 : 0;
        }

        var lowerWidth = widths[lowerWidthIdx];
        var upperWidth = widths[upperWidthIdx];

        decimal lowerHeightPpi = InterpolateHeight(lowerWidth.HeightBreakpoints, height);
        decimal upperHeightPpi = InterpolateHeight(upperWidth.HeightBreakpoints, height);

        // If both width breakpoints are the same (width fell exactly on one), no width interpolation needed
        decimal widthRange = upperWidth.Width - lowerWidth.Width;
        if (widthRange <= 0m)
            return lowerHeightPpi;

        decimal widthFraction = (width - lowerWidth.Width) / widthRange;
        return lowerHeightPpi + widthFraction * (upperHeightPpi - lowerHeightPpi);
    }

    private static decimal InterpolateHeight(List<HeightBreakpoint> heights, decimal height)
    {
        int upperIdx = heights.FindIndex(h => h.Height >= height);
        if (upperIdx < 0) upperIdx = heights.Count - 1;
        int lowerIdx = upperIdx > 0 ? upperIdx - 1 : 0;

        var lower = heights[lowerIdx];
        var upper = heights[upperIdx];

        decimal heightRange = upper.Height - lower.Height;
        if (heightRange <= 0m)
            return lower.PricePerInch;

        decimal fraction = (height - lower.Height) / heightRange;
        return lower.PricePerInch + fraction * (upper.PricePerInch - lower.PricePerInch);
    }
}
