using System.Text.Json;
using WindowConfigurator.Data.Pricing;
using Xunit;

namespace WindowConfigurator.Tests.Pricing
{
    /// <summary>
    /// Comprehensive feature combination tests for pricing validation.
    /// Uses fixed dimensions (36" x 48" base window) to focus on feature interactions,
    /// not size variations. These tests will initially fail until we resolve the 
    /// JavaScript vs C# pricing discrepancy.
    /// </summary>
    public class PricingFeatureCombinationTests
    {
        private readonly IPricingService _pricingService;
        private const decimal StandardWidth = 36m;
        private const decimal StandardHeight = 48m;

        public PricingFeatureCombinationTests()
        {
            var priceInfo = LoadPriceInfo();
            _pricingService = new PricingService(priceInfo);
        }

        #region Single Section - Frame Color Variations

        [Theory]
        [InlineData("White", "Picture", "None", "Dual")]
        [InlineData("Wicker", "Picture", "None", "Dual")]
        [InlineData("White", "Fixed Sash", "None", "Dual")]
        [InlineData("Wicker", "Fixed Sash", "None", "Dual")]
        public void SingleSection_FrameColorVariations(
            string frameColor, string style, string brickmould, string paneConfig)
        {
            var input = BuildStandardInput(frameColor, brickmould);
            input.Sections.Add(BuildStandardSection(style, paneConfig));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for {frameColor}/{style}/{brickmould}/{paneConfig}");
            LogPrice($"{frameColor}_{style}_{brickmould}_{paneConfig}", price);
        }

        #endregion

        #region Single Section - Operational Style Variations

        [Theory]
        [InlineData("Picture", "None")]
        [InlineData("Awning", "Regular")]
        [InlineData("Casement", "Regular")]
        [InlineData("Casement - Left", "Regular")]
        [InlineData("Fixed Sash", "None")]
        public void SingleSection_OperationalStyleVariations(string style, string crankType)
        {
            var input = BuildStandardInput("White", "None");
            var section = BuildStandardSection(style, "Dual");
            section.CrankName = crankType;
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for style={style}, crank={crankType}");
            LogPrice($"Style_{style}_Crank_{crankType}", price);
        }

        #endregion

        #region Single Section - Brickmould Variations

        [Theory]
        [InlineData("None")]
        [InlineData("1 1/2 Inch")]
        [InlineData("2 Inch")]
        [InlineData("1 1/2 Inch - Fin")]
        public void SingleSection_BrickmouldVariations(string brickmouldStyle)
        {
            var input = BuildStandardInput("White", brickmouldStyle);
            input.Sections.Add(BuildStandardSection("Picture", "Dual"));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for brickmould={brickmouldStyle}");
            LogPrice($"Brickmould_{brickmouldStyle.Replace(" ", "").Replace("/", "")}", price);
        }

        #endregion

        #region Single Section - Pane Configuration Variations

        [Theory]
        [InlineData("Dual")]
        [InlineData("Dual - LowE/Argon")]
        [InlineData("Triple")]
        [InlineData("Triple - LowE/Argon")]
        public void SingleSection_PaneConfigurationVariations(string paneConfig)
        {
            var input = BuildStandardInput("White", "None");
            input.Sections.Add(BuildStandardSection("Picture", paneConfig));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for paneConfig={paneConfig}");
            LogPrice($"Pane_{paneConfig.Replace(" ", "").Replace("/", "").Replace("-", "")}", price);
        }

        #endregion

        #region Single Section - Grille Pattern Variations

        [Theory]
        [InlineData("None")]
        [InlineData("Ladder")]
        [InlineData("Double Ladder")]
        [InlineData("Rectangular")]
        [InlineData("Perimeter")]
        [InlineData("Double Perimeter")]
        public void SingleSection_GrillePatternVariations(string grillePattern)
        {
            var input = BuildStandardInput("White", "None");
            var section = BuildStandardSection("Picture", "Dual");
            section.GrillePatternName = grillePattern;
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price >= 0, $"Price should be non-negative for grille={grillePattern}");
            LogPrice($"Grille_{grillePattern.Replace(" ", "")}", price);
        }

        #endregion

        #region Single Section - SDL Pattern Variations

        [Theory]
        [InlineData("None")]
        [InlineData("Ladder")]
        [InlineData("Double Ladder")]
        [InlineData("Rectangular")]
        [InlineData("Perimeter")]
        [InlineData("Double Perimeter")]
        public void SingleSection_SdlPatternVariations(string sdlPattern)
        {
            var input = BuildStandardInput("White", "None");
            var section = BuildStandardSection("Picture", "Dual");
            section.SdlPatternName = sdlPattern;
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price >= 0, $"Price should be non-negative for SDL={sdlPattern}");
            LogPrice($"SDL_{sdlPattern.Replace(" ", "")}", price);
        }

        #endregion

        #region Single Section - Crank Type Variations (Casement only)

        [Theory]
        [InlineData("Regular")]
        [InlineData("Folding")]
        [InlineData("Encore")]
        [InlineData("Roto")]
        public void SingleSection_CrankTypeVariations_Casement(string crankType)
        {
            var input = BuildStandardInput("White", "None");
            var section = BuildStandardSection("Casement", "Dual");
            section.CrankName = crankType;
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for crank={crankType}");
            LogPrice($"Crank_{crankType}_Casement", price);
        }

        #endregion

        #region Multi-Section Combinations

        [Theory]
        [InlineData("Picture", "Picture")]
        [InlineData("Picture", "Fixed Sash")]
        [InlineData("Casement", "Casement")]
        [InlineData("Casement - Left", "Casement")]
        [InlineData("Awning", "Picture")]
        public void TwoSections_StyleCombinations(string style1, string style2)
        {
            var input = BuildStandardInput("White", "None");
            
            var section1 = BuildStandardSection(style1, "Dual", StandardWidth / 2, StandardHeight);
            section1.CrankName = style1.Contains("Casement") || style1 == "Awning" ? "Regular" : "None";
            input.Sections.Add(section1);

            var section2 = BuildStandardSection(style2, "Dual", StandardWidth / 2, StandardHeight);
            section2.CrankName = style2.Contains("Casement") || style2 == "Awning" ? "Regular" : "None";
            input.Sections.Add(section2);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for {style1}/{style2}");
            LogPrice($"TwoSection_{style1.Replace(" ", "").Replace("-", "")}_{style2.Replace(" ", "").Replace("-", "")}", price);
        }

        [Fact]
        public void ThreeSections_MixedStyles()
        {
            var input = BuildStandardInput("White", "2 Inch");
            
            input.Sections.Add(BuildStandardSection("Picture", "Dual", StandardWidth / 3, StandardHeight));
            
            var casement = BuildStandardSection("Casement", "Dual - LowE/Argon", StandardWidth / 3, StandardHeight);
            casement.CrankName = "Regular";
            input.Sections.Add(casement);
            
            input.Sections.Add(BuildStandardSection("Picture", "Dual", StandardWidth / 3, StandardHeight));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for three-section window");
            LogPrice("ThreeSection_Picture_Casement_Picture", price);
        }

        #endregion

        #region Feature Combination Tests

        [Fact]
        public void FullyLoaded_AllPremiumFeatures()
        {
            var input = BuildStandardInput("Wicker", "2 Inch");
            
            var section = BuildStandardSection("Casement", "Triple - LowE/Argon");
            section.CrankName = "Encore";
            section.GrillePatternName = "Double Perimeter";
            section.SdlPatternName = "Rectangular";
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for fully loaded configuration");
            LogPrice("FullyLoaded_AllPremium", price);
        }

        [Fact]
        public void Minimal_BasicFeatures()
        {
            var input = BuildStandardInput("White", "None");
            input.Sections.Add(BuildStandardSection("Picture", "Dual"));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for minimal configuration");
            LogPrice("Minimal_WhitePictureDual", price);
        }

        [Theory]
        [InlineData("White", "None", "Dual")]
        [InlineData("White", "2 Inch", "Dual")]
        [InlineData("White", "None", "Triple - LowE/Argon")]
        [InlineData("White", "2 Inch", "Triple - LowE/Argon")]
        [InlineData("Wicker", "None", "Dual")]
        [InlineData("Wicker", "2 Inch", "Dual")]
        [InlineData("Wicker", "None", "Triple - LowE/Argon")]
        [InlineData("Wicker", "2 Inch", "Triple - LowE/Argon")]
        public void CommonConfigurations_FrameBrickmouldPaneCombos(
            string frameColor, string brickmould, string paneConfig)
        {
            var input = BuildStandardInput(frameColor, brickmould);
            input.Sections.Add(BuildStandardSection("Picture", paneConfig));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for {frameColor}/{brickmould}/{paneConfig}");
            LogPrice($"Common_{frameColor}_{brickmould.Replace(" ", "")}_{paneConfig.Replace(" ", "").Replace("/", "").Replace("-", "")}", price);
        }

        [Theory]
        [InlineData("Casement", "Regular", "Ladder", "None")]
        [InlineData("Casement", "Regular", "None", "Ladder")]
        [InlineData("Casement", "Regular", "Ladder", "Ladder")]
        [InlineData("Casement", "Encore", "Double Perimeter", "Rectangular")]
        [InlineData("Awning", "Folding", "Perimeter", "None")]
        public void StylesWithGrillesAndSdl(
            string style, string crank, string grille, string sdl)
        {
            var input = BuildStandardInput("White", "1 1/2 Inch");
            var section = BuildStandardSection(style, "Dual - LowE/Argon");
            section.CrankName = crank;
            section.GrillePatternName = grille;
            section.SdlPatternName = sdl;
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, $"Price should be positive for {style}/{crank}/{grille}/{sdl}");
            LogPrice($"{style.Replace(" ", "")}_{crank}_{grille.Replace(" ", "")}_{sdl.Replace(" ", "")}", price);
        }

        #endregion

        #region Multi-Section with Mixed Features

        [Fact]
        public void TwoSections_DifferentPaneConfigs()
        {
            var input = BuildStandardInput("White", "None");
            input.Sections.Add(BuildStandardSection("Picture", "Dual", StandardWidth / 2, StandardHeight));
            input.Sections.Add(BuildStandardSection("Picture", "Triple - LowE/Argon", StandardWidth / 2, StandardHeight));

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for mixed pane configs");
            LogPrice("TwoSection_MixedPaneConfigs", price);
        }

        [Fact]
        public void TwoSections_DifferentGrilles()
        {
            var input = BuildStandardInput("White", "None");
            
            var section1 = BuildStandardSection("Picture", "Dual", StandardWidth / 2, StandardHeight);
            section1.GrillePatternName = "None";
            input.Sections.Add(section1);

            var section2 = BuildStandardSection("Picture", "Dual", StandardWidth / 2, StandardHeight);
            section2.GrillePatternName = "Ladder";
            input.Sections.Add(section2);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for mixed grille patterns");
            LogPrice("TwoSection_MixedGrilles", price);
        }

        [Fact]
        public void TwoSections_OneCasementOnePicture_WithBrickmould()
        {
            var input = BuildStandardInput("Wicker", "2 Inch");
            
            var casement = BuildStandardSection("Casement", "Dual - LowE/Argon", StandardWidth / 2, StandardHeight);
            casement.CrankName = "Folding";
            casement.GrillePatternName = "Perimeter";
            input.Sections.Add(casement);

            var picture = BuildStandardSection("Picture", "Dual - LowE/Argon", StandardWidth / 2, StandardHeight);
            picture.GrillePatternName = "Perimeter";
            input.Sections.Add(picture);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for Casement/Picture combo with brickmould");
            LogPrice("TwoSection_CasementPicture_Wicker2InchBrickmould", price);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void EdgeCase_NoBrickmould_NoGrilles_NoSdl_BasicDual()
        {
            var input = BuildStandardInput("White", "None");
            var section = BuildStandardSection("Picture", "Dual");
            section.GrillePatternName = "None";
            section.SdlPatternName = "None";
            input.Sections.Add(section);

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for absolute minimal config");
            LogPrice("EdgeCase_AbsoluteMinimal", price);
        }

        [Fact]
        public void EdgeCase_MaximumFeatures_FourSections()
        {
            var input = BuildStandardInput("Wicker", "2 Inch");
            
            for (int i = 0; i < 4; i++)
            {
                var section = BuildStandardSection("Picture", "Triple - LowE/Argon", StandardWidth / 4, StandardHeight);
                section.GrillePatternName = "Double Perimeter";
                section.SdlPatternName = "Rectangular";
                input.Sections.Add(section);
            }

            var price = _pricingService.CalculatePrice(input);

            Assert.True(price > 0, "Price should be positive for four-section loaded window");
            LogPrice("EdgeCase_FourSections_MaxFeatures", price);
        }

        #endregion

        #region Helpers

        private static WindowPricingInput BuildStandardInput(string frameColor, string brickmouldStyle)
        {
            return new WindowPricingInput
            {
                ManufacturerName = "All Weather Windows",
                ProductLineName = "EnergySaver 2500",
                FrameWidthDecimal = StandardWidth,
                FrameHeightDecimal = StandardHeight,
                FrameColorName = frameColor,
                BrickmouldStyleName = brickmouldStyle
            };
        }

        private static SectionPricingInput BuildStandardSection(
            string style, 
            string paneConfig, 
            decimal? width = null, 
            decimal? height = null)
        {
            return new SectionPricingInput
            {
                WidthDecimal = width ?? StandardWidth,
                HeightDecimal = height ?? StandardHeight,
                StyleName = style,
                GrillePatternName = "None",
                SdlPatternName = "None",
                CrankName = "None",
                PaneConfigurationName = paneConfig
            };
        }

        private static void LogPrice(string testCase, decimal price)
        {
            Console.WriteLine($"{testCase}: ${price:F2}");
        }

        private static PriceInfoRoot LoadPriceInfo()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", "priceInfo.json");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PriceInfoRoot>(
                json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        }

        #endregion
    }
}
