namespace WindowConfigurator.Data.Pricing;

public class PriceInfoRoot
{
    public PriceInfoData PriceInfo { get; set; } = null!;
}

public class PriceInfoData
{
    public List<ManufacturerPricing> Manufacturers { get; set; } = [];
}

public class ManufacturerPricing
{
    public string Name { get; set; } = string.Empty;
    public List<ProductLinePricing> ProductLines { get; set; } = [];
}

public class ProductLinePricing
{
    public string Name { get; set; } = string.Empty;
    public decimal Adjustment { get; set; }
    public decimal Markup { get; set; }
    public List<FrameColorPricing> FrameColors { get; set; } = [];
    public List<CrankPricing> Cranks { get; set; } = [];
    public List<JambDepthPricing> JambDepths { get; set; } = [];

    /// <summary>Operational style pricing — e.g., Casement, Picture, Awning.</summary>
    public List<BreakpointPricedItem> Styles { get; set; } = [];
    public List<BreakpointPricedItem> GrillePatterns { get; set; } = [];
    public List<BreakpointPricedItem> SdlPatterns { get; set; } = [];
    public List<BreakpointPricedItem> BrickmouldStyles { get; set; } = [];
    public List<BreakpointPricedItem> PaneConfigurations { get; set; } = [];
}

/// <summary>Anything priced via 2D width/height breakpoint interpolation.</summary>
public class BreakpointPricedItem
{
    public string Name { get; set; } = string.Empty;
    public List<WidthBreakpoint> WidthBreakpoints { get; set; } = [];
}

public class WidthBreakpoint
{
    public decimal Width { get; set; }
    public List<HeightBreakpoint> HeightBreakpoints { get; set; } = [];
}

public class HeightBreakpoint
{
    public decimal Height { get; set; }
    public decimal PricePerInch { get; set; }
}

public class FrameColorPricing
{
    public string Name { get; set; } = string.Empty;
    public decimal PricePerInch { get; set; }
}

public class CrankPricing
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/// <summary>Jamb depth is in the data but pricing is not yet implemented (no breakpoints).</summary>
public class JambDepthPricing
{
    public string Name { get; set; } = string.Empty;
}
