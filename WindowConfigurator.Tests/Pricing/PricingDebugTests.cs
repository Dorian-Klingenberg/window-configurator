using System.Text.Json;
using WindowConfigurator.Data.Pricing;
using Xunit;

namespace WindowConfigurator.Tests.Pricing
{
    public class PricingDebugTests
    {
        [Fact]
        public void InspectBuggyConfiguration()
        {
            var priceInfo = LoadPriceInfo();
            var pl = priceInfo.PriceInfo.Manufacturers
                .First(m => m.Name == "All Weather Windows")
                .ProductLines.First(p => p.Name == "EnergySaver 2500");

            // Check that Picture style exists and has breakpoints
            var pictureStyle = pl.Styles.FirstOrDefault(s => s.Name == "Picture");
            Assert.NotNull(pictureStyle);
            Console.WriteLine($"Picture style found with {pictureStyle.WidthBreakpoints.Count} width breakpoints");

            // Check frame color price (should be 0 for White)
            var whiteColor = pl.FrameColors.FirstOrDefault(c => c.Name == "White");
            Assert.NotNull(whiteColor);
            Console.WriteLine($"White frame color: pricePerInch = {whiteColor.PricePerInch}");

            // Check brickmould
            var brickmould2Inch = pl.BrickmouldStyles.FirstOrDefault(b => b.Name == "2 Inch");
            Assert.NotNull(brickmould2Inch);
            Console.WriteLine($"2 Inch brickmould: {brickmould2Inch.WidthBreakpoints.Count} width breakpoints");

            // Check pane configuration
            var dual = pl.PaneConfigurations.FirstOrDefault(p => p.Name == "Dual");
            Assert.NotNull(dual);
            Console.WriteLine($"Dual pane: {dual.WidthBreakpoints.Count} width breakpoints");

            // Now let's manually calculate what each component should contribute
            Console.WriteLine("\n=== MANUAL CALCULATION ===");

            // Frame perimeter: (77 + 43.375) * 2 = 240.75 inches
            decimal framePerimeter = (77m + 43.375m) * 2;
            Console.WriteLine($"Frame perimeter: {framePerimeter} inches");
            decimal frameColorPrice = framePerimeter * whiteColor.PricePerInch;
            Console.WriteLine($"Frame color price: {framePerimeter} * {whiteColor.PricePerInch} = ${frameColorPrice}");

            // Section 1: 27.375 x 44.0625
            decimal section1Perimeter = (27.375m * 2) + (44.0625m * 2);
            var section1Ppi = PriceInterpolator.Interpolate(pictureStyle, 27.375m, 44.0625m);
            decimal section1Price = section1Perimeter * section1Ppi;
            Console.WriteLine($"\nSection 1: {section1Perimeter} inch perimeter * ${section1Ppi} PPI = ${section1Price}");

            // Section 2: 48.4375 x 44.0625
            decimal section2Perimeter = (48.4375m * 2) + (44.0625m * 2);
            var section2Ppi = PriceInterpolator.Interpolate(pictureStyle, 48.4375m, 44.0625m);
            decimal section2Price = section2Perimeter * section2Ppi;
            Console.WriteLine($"Section 2: {section2Perimeter} inch perimeter * ${section2Ppi} PPI = ${section2Price}");

            // Brickmould: 77 x 43.375 frame perimeter
            var brickmouldPpi = PriceInterpolator.Interpolate(brickmould2Inch, 77m, 43.375m);
            decimal brickmouldPrice = framePerimeter * brickmouldPpi;
            Console.WriteLine($"\nBrickmould: {framePerimeter} inch perimeter * ${brickmouldPpi} PPI = ${brickmouldPrice}");

            // Pane config for each section
            var pane1Ppi = PriceInterpolator.Interpolate(dual, 27.375m, 44.0625m);
            decimal pane1Price = section1Perimeter * pane1Ppi;
            Console.WriteLine($"\nPane 1: {section1Perimeter} inch perimeter * ${pane1Ppi} PPI = ${pane1Price}");

            var pane2Ppi = PriceInterpolator.Interpolate(dual, 48.4375m, 44.0625m);
            decimal pane2Price = section2Perimeter * pane2Ppi;
            Console.WriteLine($"Pane 2: {section2Perimeter} inch perimeter * ${pane2Ppi} PPI = ${pane2Price}");

            decimal subtotal = frameColorPrice + section1Price + section2Price + brickmouldPrice + pane1Price + pane2Price;
            Console.WriteLine($"\nSubtotal before adjustment/markup: ${Math.Round(subtotal, 2)}");

            decimal afterAdjustment = subtotal + (subtotal * pl.Adjustment);
            Console.WriteLine($"After adjustment ({pl.Adjustment * 100}%): ${Math.Round(afterAdjustment, 2)}");

            decimal final = afterAdjustment + (afterAdjustment * pl.Markup);
            Console.WriteLine($"After markup ({pl.Markup * 100}%): ${Math.Round(final, 2)}");

            Console.WriteLine($"\n=== EXPECTED (JS): $1812.33 ===");
            Console.WriteLine($"=== ACTUAL (C#):   ${Math.Round(final, 2)} ===");
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
