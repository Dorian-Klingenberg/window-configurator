using System.Globalization;
using System.Text.Json;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Pricing;

namespace WindowConfigurator.Data.Validation;

public sealed class CompletionValidationService : ICompletionValidationService
{
    private readonly PriceInfoRoot _priceInfo;

    public CompletionValidationService(PriceInfoRoot priceInfo)
    {
        _priceInfo = priceInfo;
    }

    public CompletionValidationResult Validate(
        JsonElement payload,
        string productLineKey,
        TenantEntity? tenant,
        string itemTemplateJson)
    {
        var errors = new List<CompletionValidationError>();

        ValidateTenantProductLine(productLineKey, tenant, errors);

        using var template = JsonDocument.Parse(itemTemplateJson);
        if (!template.RootElement.TryGetProperty("productLine", out var productLine) ||
            productLine.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new("productLine", "The catalog template does not contain a product line."));
            return CompletionValidationResult.Failed(errors);
        }

        ValidateFrame(payload, productLine, errors);
        ValidateOption(payload, productLine, "frameColor", "frameColors", errors);
        ValidateOption(payload, productLine, "brickmouldStyle", "brickmouldStyles", errors);
        ValidateOption(payload, productLine, "paneConfiguration", "paneConfigurations", errors);
        ValidateSections(payload, productLine, errors);
        ValidatePricingBreakpoints(payload, productLine, errors);

        return errors.Count == 0
            ? CompletionValidationResult.Success()
            : CompletionValidationResult.Failed(errors);
    }

    private static void ValidateTenantProductLine(
        string productLineKey,
        TenantEntity? tenant,
        List<CompletionValidationError> errors)
    {
        if (tenant == null || tenant.AllowedProductLineKeys.Count == 0)
            return;

        if (!tenant.AllowedProductLineKeys.Contains(productLineKey))
        {
            errors.Add(new(
                "productLine",
                $"Product line '{productLineKey}' is not available for this tenant."));
        }
    }

    private static void ValidateFrame(
        JsonElement payload,
        JsonElement productLine,
        List<CompletionValidationError> errors)
    {
        if (!productLine.TryGetProperty("frameRestrictions", out var restrictions))
            return;

        ValidateMeasurementRange(payload, "frameWidth", restrictions, "minWidth", "maxWidth", errors);
        ValidateMeasurementRange(payload, "frameHeight", restrictions, "minHeight", "maxHeight", errors);
    }

    private static void ValidateSections(
        JsonElement payload,
        JsonElement productLine,
        List<CompletionValidationError> errors)
    {
        if (!payload.TryGetProperty("sections", out var sections) || sections.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new("sections", "At least one section is required."));
            return;
        }

        if (!productLine.TryGetProperty("operationalStyles", out var styles) ||
            styles.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new("style", "The product line does not define operational styles."));
            return;
        }

        var index = 0;
        foreach (var section in sections.EnumerateArray())
        {
            var styleName = ReadNestedString(section, "style", "name");
            var style = FindNamedElement(styles, styleName);
            if (style == null)
            {
                errors.Add(new($"sections[{index}].style", $"Style '{styleName}' is not available for this product line."));
            }
            else if (style.Value.TryGetProperty("restrictions", out var restrictions))
            {
                ValidateMeasurementRange(section, $"sections[{index}].width", "width", restrictions, "minWidth", "maxWidth", errors);
                ValidateMeasurementRange(section, $"sections[{index}].height", "height", restrictions, "minHeight", "maxHeight", errors);
            }

            ValidateOption(section, productLine, $"sections[{index}].grillePattern", "grillePattern", "grillePatterns", errors);
            ValidateOption(section, productLine, $"sections[{index}].sdlPattern", "sdlPattern", "sdlPatterns", errors);
            index++;
        }
    }

    private void ValidatePricingBreakpoints(
        JsonElement payload,
        JsonElement productLine,
        List<CompletionValidationError> errors)
    {
        var pricing = FindPricingProductLine(productLine);
        if (pricing == null)
        {
            errors.Add(new("productLine", "The selected product line does not have pricing data."));
            return;
        }

        if (!payload.TryGetProperty("sections", out var sections) || sections.ValueKind != JsonValueKind.Array)
            return;

        var paneConfigurationName = ReadNestedString(payload, "paneConfiguration", "name");
        var paneConfiguration = FindPricedItem(pricing.PaneConfigurations, paneConfigurationName);

        var index = 0;
        foreach (var section in sections.EnumerateArray())
        {
            var width = ReadMeasurement(section, "width");
            var height = ReadMeasurement(section, "height");
            if (width == null || height == null)
            {
                index++;
                continue;
            }

            ValidatePricedItem(
                $"sections[{index}].style",
                FindPricedItem(pricing.Styles, ReadNestedString(section, "style", "name")),
                width.Value,
                height.Value,
                errors);

            ValidatePricedItem(
                $"sections[{index}].grillePattern",
                FindPricedItem(pricing.GrillePatterns, ReadNestedString(section, "grillePattern", "name")),
                width.Value,
                height.Value,
                errors);

            ValidatePricedItem(
                $"sections[{index}].sdlPattern",
                FindPricedItem(pricing.SdlPatterns, ReadNestedString(section, "sdlPattern", "name")),
                width.Value,
                height.Value,
                errors);

            ValidatePricedItem(
                $"sections[{index}].paneConfiguration",
                paneConfiguration,
                width.Value,
                height.Value,
                errors);

            index++;
        }

        var brickmouldWidth = ReadMeasurement(payload, "width") ?? ReadMeasurement(payload, "frameWidth");
        var brickmouldHeight = ReadMeasurement(payload, "height") ?? ReadMeasurement(payload, "frameHeight");
        if (brickmouldWidth != null && brickmouldHeight != null)
        {
            ValidatePricedItem(
                "brickmouldStyle",
                FindPricedItem(pricing.BrickmouldStyles, ReadNestedString(payload, "brickmouldStyle", "name")),
                brickmouldWidth.Value,
                brickmouldHeight.Value,
                errors);
        }
    }

    private ProductLinePricing? FindPricingProductLine(JsonElement productLine)
    {
        var manufacturerName = ReadString(productLine, "manufacturerName");
        var productLineName = ReadString(productLine, "name");

        var manufacturer = _priceInfo.PriceInfo.Manufacturers.FirstOrDefault(m =>
            string.Equals(m.Name, manufacturerName, StringComparison.OrdinalIgnoreCase));

        return manufacturer?.ProductLines.FirstOrDefault(p =>
            string.Equals(p.Name, productLineName, StringComparison.OrdinalIgnoreCase));
    }

    private static BreakpointPricedItem? FindPricedItem(List<BreakpointPricedItem> items, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return items.FirstOrDefault(item =>
            string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static void ValidatePricedItem(
        string fieldName,
        BreakpointPricedItem? item,
        decimal width,
        decimal height,
        List<CompletionValidationError> errors)
    {
        if (item == null || item.WidthBreakpoints.Count == 0)
            return;

        if (IsPastFinalBreakpoint(item, width, height))
        {
            errors.Add(new(
                fieldName,
                $"{fieldName} dimensions exceed the supported pricing grid."));
        }
    }

    private static bool IsPastFinalBreakpoint(BreakpointPricedItem item, decimal width, decimal height)
    {
        var widths = item.WidthBreakpoints;
        var maxWidth = widths[^1].Width;
        if (width > maxWidth)
            return true;

        var (lowerWidth, upperWidth) = GetBoundingWidthBreakpoints(widths, width);
        var lowerMaxHeight = lowerWidth.HeightBreakpoints[^1].Height;
        var upperMaxHeight = upperWidth.HeightBreakpoints[^1].Height;

        return height > Math.Min(lowerMaxHeight, upperMaxHeight);
    }

    private static (WidthBreakpoint Lower, WidthBreakpoint Upper) GetBoundingWidthBreakpoints(
        List<WidthBreakpoint> widths,
        decimal width)
    {
        var lower = widths[0];
        var upper = widths[0];

        foreach (var candidate in widths)
        {
            upper = candidate;
            if (width >= upper.Width)
                lower = upper;
            else
                break;
        }

        return (lower, upper);
    }

    private static void ValidateOption(
        JsonElement payload,
        JsonElement productLine,
        string payloadProperty,
        string catalogProperty,
        List<CompletionValidationError> errors)
    {
        ValidateOption(payload, productLine, payloadProperty, payloadProperty, catalogProperty, errors);
    }

    private static void ValidateOption(
        JsonElement payload,
        JsonElement productLine,
        string fieldName,
        string payloadProperty,
        string catalogProperty,
        List<CompletionValidationError> errors)
    {
        var selectedName = ReadNestedString(payload, payloadProperty, "name");
        if (string.IsNullOrWhiteSpace(selectedName))
        {
            errors.Add(new(fieldName, $"A {payloadProperty} selection is required."));
            return;
        }

        if (!productLine.TryGetProperty(catalogProperty, out var options) ||
            options.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new(fieldName, $"The catalog does not define {catalogProperty}."));
            return;
        }

        if (FindNamedElement(options, selectedName) == null)
        {
            errors.Add(new(fieldName, $"'{selectedName}' is not available for this product line."));
        }
    }

    private static void ValidateMeasurementRange(
        JsonElement payload,
        string propertyName,
        JsonElement restrictions,
        string minProperty,
        string maxProperty,
        List<CompletionValidationError> errors)
    {
        ValidateMeasurementRange(payload, propertyName, propertyName, restrictions, minProperty, maxProperty, errors);
    }

    private static void ValidateMeasurementRange(
        JsonElement payload,
        string fieldName,
        string propertyName,
        JsonElement restrictions,
        string minProperty,
        string maxProperty,
        List<CompletionValidationError> errors)
    {
        var value = ReadMeasurement(payload, propertyName);
        if (value == null)
        {
            errors.Add(new(fieldName, $"{fieldName} is required."));
            return;
        }

        var min = ReadMeasurement(restrictions, minProperty);
        var max = ReadMeasurement(restrictions, maxProperty);

        if (min != null && value < min)
            errors.Add(new(fieldName, $"{fieldName} is below the minimum of {min.Value.ToString(CultureInfo.InvariantCulture)} inches."));

        if (max != null && value > max)
            errors.Add(new(fieldName, $"{fieldName} is above the maximum of {max.Value.ToString(CultureInfo.InvariantCulture)} inches."));
    }

    private static JsonElement? FindNamedElement(JsonElement options, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        foreach (var option in options.EnumerateArray())
        {
            if (option.TryGetProperty("name", out var element) &&
                element.ValueKind == JsonValueKind.String &&
                string.Equals(element.GetString(), name, StringComparison.OrdinalIgnoreCase))
            {
                return option;
            }
        }

        return null;
    }

    private static string? ReadNestedString(JsonElement payload, string parentProperty, string childProperty)
    {
        if (!payload.TryGetProperty(parentProperty, out var parent) || parent.ValueKind != JsonValueKind.Object)
            return null;

        return parent.TryGetProperty(childProperty, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? ReadString(JsonElement payload, string propertyName)
    {
        return payload.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static decimal? ReadMeasurement(JsonElement payload, string propertyName)
    {
        if (!payload.TryGetProperty(propertyName, out var measurement) ||
            measurement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!ReadDecimal(measurement, "sign", out var sign) ||
            !ReadDecimal(measurement, "whole", out var whole) ||
            !ReadDecimal(measurement, "numerator", out var numerator) ||
            !ReadDecimal(measurement, "denominator", out var denominator) ||
            denominator == 0m)
        {
            return null;
        }

        var magnitude = whole + (numerator / denominator);
        return sign < 0 ? -magnitude : magnitude;
    }

    private static bool ReadDecimal(JsonElement payload, string propertyName, out decimal value)
    {
        value = default;
        if (!payload.TryGetProperty(propertyName, out var element))
            return false;

        if (element.ValueKind == JsonValueKind.Number)
            return element.TryGetDecimal(out value);

        if (element.ValueKind == JsonValueKind.String)
            return decimal.TryParse(element.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

        return false;
    }
}
