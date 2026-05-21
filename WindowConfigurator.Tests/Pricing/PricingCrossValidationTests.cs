using System.Text.Json;
using WindowConfigurator.Data.Pricing;
using Xunit;

namespace WindowConfigurator.Tests.Pricing
{
    /// <summary>
    /// Cross-validation tests comparing C# PricingService against known JavaScript results.
    /// These tests use real-world configurations and their expected JS prices to verify
    /// the C# port matches the original pricing engine.
    /// </summary>
    public class PricingCrossValidationTests
    {
        private readonly IPricingService _pricingService;

        public PricingCrossValidationTests()
        {
            var priceInfo = LoadPriceInfo();
            _pricingService = new PricingService(priceInfo);
        }

        [Fact]
        public void LargeWindow_TwoSections_2InchBrickmould_MatchesJavaScript()
        {
            // Cross-validated against the legacy browser pricing engine by executing
            // pricing.js directly with the same measurements and priceInfo.json data.
            // The JavaScript engine returns $1066.75 for this configuration.

            var input = new WindowPricingInput
            {
                ManufacturerName = "All Weather Windows",
                ProductLineName = "EnergySaver 2500",
                FrameWidthDecimal = 77m,
                FrameHeightDecimal = 43.375m,
                OutsideWidthDecimal = 77m,
                OutsideHeightDecimal = 43.375m,
                FrameColorName = "White",
                BrickmouldStyleName = "2 Inch"
            };

            input.Sections.Add(new SectionPricingInput
            {
                WidthDecimal = 27.375m,       // 27 3/8
                HeightDecimal = 44.0625m,     // 44 1/16
                StyleName = "Picture",
                GrillePatternName = "None",
                SdlPatternName = "None",
                CrankName = "None",
                PaneConfigurationName = "Dual"
            });

            input.Sections.Add(new SectionPricingInput
            {
                WidthDecimal = 48.4375m,      // 48 7/16 (from payload, though widthDescription said "34 5/8")
                HeightDecimal = 44.0625m,     // 44 1/16
                StyleName = "Picture",
                GrillePatternName = "None",
                SdlPatternName = "None",
                CrankName = "None",
                PaneConfigurationName = "Dual"
            });

            var csharpPrice = _pricingService.CalculatePrice(input);

            // The JavaScript engine calculated $1066.75.
            try
            {
                Assert.Equal(1066.75m, csharpPrice);
            }
            catch
            {
                // Log detailed breakdown when the test fails
                Console.WriteLine($"\n=== PRICING BREAKDOWN DEBUG ===");
                Console.WriteLine($"Expected (JS): $1066.75");
                Console.WriteLine($"Actual (C#):   ${csharpPrice}");
                Console.WriteLine($"Difference:    ${1066.75m - csharpPrice}");
                Console.WriteLine($"Frame: {input.FrameWidthDecimal}\" x {input.FrameHeightDecimal}\"");
                Console.WriteLine($"Section 1: {input.Sections[0].WidthDecimal}\" x {input.Sections[0].HeightDecimal}\"");
                Console.WriteLine($"Section 2: {input.Sections[1].WidthDecimal}\" x {input.Sections[1].HeightDecimal}\"");
                throw;
            }
        }

        [Fact]
        public void SmallWindow_SingleSection_NoBrickmould_MatchesJavaScript()
        {
            // Baseline test from existing test suite - this one passes
            var input = new WindowPricingInput
            {
                ManufacturerName = "All Weather Windows",
                ProductLineName = "EnergySaver 2500",
                FrameWidthDecimal = 24m,
                FrameHeightDecimal = 36m,
                FrameColorName = "White",
                BrickmouldStyleName = "None"
            };

            input.Sections.Add(new SectionPricingInput
            {
                WidthDecimal = 24m,
                HeightDecimal = 36m,
                StyleName = "Casement",
                GrillePatternName = "None",
                SdlPatternName = "None",
                CrankName = "None",
                PaneConfigurationName = "Dual"
            });

            var csharpPrice = _pricingService.CalculatePrice(input);

            // This matches the existing test expectation
            Assert.Equal(351.12m, csharpPrice);
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
}
