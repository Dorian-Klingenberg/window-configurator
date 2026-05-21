namespace WindowConfigurator.Data.Pricing;

/// <summary>
/// All pricing-relevant data for a configured window, extracted from ConfigurationJson
/// and passed to IPricingService at completion time.
/// </summary>
public class WindowPricingInput
{
    public string ManufacturerName { get; set; } = string.Empty;
    public string ProductLineName { get; set; } = string.Empty;

    /// <summary>Frame width in decimal inches.</summary>
    public decimal FrameWidthDecimal { get; set; }

    /// <summary>Frame height in decimal inches.</summary>
    public decimal FrameHeightDecimal { get; set; }

    /// <summary>
    /// Dimensions used by the legacy JavaScript brickmould calculator. In brickmould sizing
    /// mode these are the payload's top-level width/height values, which differ from frame
    /// width/height.
    /// </summary>
    public decimal BrickmouldPricingWidthDecimal { get; set; }
    public decimal BrickmouldPricingHeightDecimal { get; set; }

    /// <summary>
    /// Outside measurement width in decimal inches.
    /// The legacy JavaScript pricing engine uses outside dimensions for frame color pricing.
    /// </summary>
    public decimal OutsideWidthDecimal { get; set; }

    /// <summary>
    /// Outside measurement height in decimal inches.
    /// The legacy JavaScript pricing engine uses outside dimensions for frame color pricing.
    /// </summary>
    public decimal OutsideHeightDecimal { get; set; }

    public string FrameColorName { get; set; } = string.Empty;
    public string BrickmouldStyleName { get; set; } = string.Empty;

    public List<SectionPricingInput> Sections { get; set; } = [];
}

public class SectionPricingInput
{
    public decimal WidthDecimal { get; set; }
    public decimal HeightDecimal { get; set; }
    public string StyleName { get; set; } = string.Empty;
    public string GrillePatternName { get; set; } = string.Empty;
    public string SdlPatternName { get; set; } = string.Empty;
    public string CrankName { get; set; } = string.Empty;
    public string PaneConfigurationName { get; set; } = string.Empty;
}
