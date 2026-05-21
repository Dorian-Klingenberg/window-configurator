using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using WindowConfigurator.Controllers;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Pricing;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Validation;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Controllers
{
    public class OrderItemControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly IQuoteSessionRepository _sessionRepo;
        private readonly ITenantRepository _tenantRepo;
        private readonly ICatalogService _catalog;
        private readonly IPricingService _pricingService;
        private readonly ICompletionValidationService _completionValidation;
        private readonly FakeTemplateReader _fakeReader;
        private readonly OrderItemController _controller;

        public OrderItemControllerTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();

            _sessionRepo = new EfQuoteSessionRepository(_context);
            _tenantRepo = new EfTenantRepository(_context);
            _catalog = new CatalogService();
            var priceInfo = LoadPriceInfo();
            _pricingService = new PricingService(priceInfo);
            _completionValidation = new CompletionValidationService(priceInfo);
            _fakeReader = new FakeTemplateReader();
            _controller = new OrderItemController(
                _sessionRepo,
                _catalog,
                _fakeReader,
                _pricingService,
                _tenantRepo,
                _completionValidation,
                priceInfo);
        }

        private async Task<QuoteSessionEntity> SeedSessionWithProductLine(
            string productLineKey,
            List<string>? allowedProductLineKeys = null)
        {
            var tenant = new TenantEntity
            {
                Name = "Test Dealer",
                ApiKey = Guid.NewGuid().ToString(),
                AllowedProductLineKeys = allowedProductLineKeys ?? []
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var session = new QuoteSessionEntity
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = productLineKey
            };
            await _sessionRepo.AddAsync(session);
            await _sessionRepo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity
            {
                ProductLineKey = productLineKey,
                Location = "Living Room"
            });
            await _sessionRepo.SaveChangesAsync();
            return session;
        }

        [Fact]
        public async Task GetOverview_WithApexSession_ReturnsApexTemplate()
        {
            var session = await SeedSessionWithProductLine("apex");

            var result = await _controller.GetOverview(session.Id.ToString()) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal("apexItemTemplate.json content", result.Value);
        }

        [Fact]
        public async Task GetOverview_WithEnergySaverSession_ReturnsEnergySaverTemplate()
        {
            var session = await SeedSessionWithProductLine("energysaver-2500");

            var result = await _controller.GetOverview(session.Id.ToString()) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal("energySaverItemTemplate.json content", result.Value);
        }

        [Fact]
        public async Task GetOverview_WithUnknownSessionId_Returns404()
        {
            var result = await _controller.GetOverview(Guid.NewGuid().ToString());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetOverview_WithNullOrEmptyId_Returns404()
        {
            var result = await _controller.GetOverview(string.Empty);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_WithInvalidId_RedirectsToFirstAvailableSession()
        {
            var session = await SeedSessionWithProductLine("energysaver-2500");

            var result = await _controller.Index("not-a-guid");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(session.Id.ToString(), redirect.RouteValues!["id"]?.ToString());
        }

        [Fact]
        public async Task Index_WithUnknownGuid_RedirectsToFirstAvailableSession()
        {
            var session = await SeedSessionWithProductLine("energysaver-2500");

            var result = await _controller.Index(Guid.NewGuid().ToString());

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(session.Id.ToString(), redirect.RouteValues!["id"]?.ToString());
        }

        [Fact]
        public async Task Index_WithInvalidIdAndNoSessions_Returns404()
        {
            var result = await _controller.Index("not-a-guid");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SectionTemplate_WithCarriageSession_ReturnsCarriageSectionTemplate()
        {
            var session = await SeedSessionWithProductLine("carriage");

            var result = await _controller.SectionTemplate(session.Id.ToString()) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal("carriageSectionTemplate.json content", result.Value);
        }

        [Fact]
        public async Task SectionTemplate_WithUnknownSessionId_Returns404()
        {
            var result = await _controller.SectionTemplate(Guid.NewGuid().ToString());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PriceInfo_ReturnsAlignedBrickmouldGridPastPreviousEdge()
        {
            var result = await _controller.PriceInfo(Guid.NewGuid().ToString());
            var ok = Assert.IsType<OkObjectResult>(result);
            var root = Assert.IsType<PriceInfoRoot>(ok.Value);

            var manufacturer = Assert.Single(root.PriceInfo.Manufacturers, m => m.Name == "All Weather Windows");
            var energySaver = Assert.Single(manufacturer.ProductLines, pl => pl.Name == "EnergySaver 2500");
            var twoInchBrickmould = Assert.Single(energySaver.BrickmouldStyles, b => b.Name == "2 Inch");

            Assert.True(twoInchBrickmould.WidthBreakpoints[^1].Width >= 90m);
            Assert.True(twoInchBrickmould.WidthBreakpoints[^1].HeightBreakpoints[^1].Height >= 46.0625m);
        }

        [Fact]
        public async Task Complete_WithExistingSession_ComputesAuthoritativePriceAndMarksSessionComplete()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var persisted = await _sessionRepo.GetByIdAsync(session.Id);
            var existingItemId = Assert.Single(persisted!.Items).Id;

            var payload = BuildValidCompletionPayload(session.Id, existingItemId);

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal(351.12m, GetDecimalProperty(okResult.Value!, "authoritativePrice"));
            Assert.Equal("Completed", GetStringProperty(okResult.Value!, "sessionStatus"));

            var reloaded = await _sessionRepo.GetByIdAsync(session.Id);
            var item = Assert.Single(reloaded!.Items);

            Assert.Equal(ConfiguredWindowItemStatus.Completed, item.Status);
            Assert.Equal(351.12m, item.AuthoritativePrice);
            Assert.NotNull(item.PricingComputedAt);
            Assert.NotNull(item.CompletedAt);
            Assert.Equal(QuoteSessionStatus.Completed, reloaded.Status);
            Assert.NotNull(reloaded.CompletedAt);
        }

        [Fact]
        public async Task Complete_WithFrameWidthBelowCatalogMinimum_ReturnsValidationError()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var itemId = Assert.Single((await _sessionRepo.GetByIdAsync(session.Id))!.Items).Id;
            var payload = BuildValidCompletionPayload(
                session.Id,
                itemId,
                frameWidthWhole: 10,
                sectionWidthWhole: 10);

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("frameWidth", JsonSerializer.Serialize(badRequest.Value));

            var reloaded = await _sessionRepo.GetByIdAsync(session.Id);
            Assert.Equal(QuoteSessionStatus.Draft, reloaded!.Status);
            Assert.Null(Assert.Single(reloaded.Items).AuthoritativePrice);
        }

        [Fact]
        public async Task Complete_WithUnsupportedStyle_ReturnsValidationError()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var itemId = Assert.Single((await _sessionRepo.GetByIdAsync(session.Id))!.Items).Id;
            var payload = BuildValidCompletionPayload(session.Id, itemId, styleName: "Imaginary Style");

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("style", JsonSerializer.Serialize(badRequest.Value));
        }

        [Fact]
        public async Task Complete_WithUnsupportedFrameColor_ReturnsValidationError()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var itemId = Assert.Single((await _sessionRepo.GetByIdAsync(session.Id))!.Items).Id;
            var payload = BuildValidCompletionPayload(session.Id, itemId, frameColorName: "Invisible");

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("frameColor", JsonSerializer.Serialize(badRequest.Value));
        }

        [Fact]
        public async Task Complete_WithProductLineOutsideTenantCatalog_ReturnsValidationError()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine(
                "energysaver-2500",
                ["energysaver-2500"]);
            var itemId = Assert.Single((await _sessionRepo.GetByIdAsync(session.Id))!.Items).Id;
            var payload = BuildValidCompletionPayload(
                session.Id,
                itemId,
                productLineKey: "apex",
                productLineName: "Apex");

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("productLine", JsonSerializer.Serialize(badRequest.Value));
        }

        [Fact]
        public async Task Complete_WithMixedProductLinesDisallowed_ReturnsValidationError()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine(
                "energysaver-2500",
                ["energysaver-2500", "apex"]);

            var tenant = await _tenantRepo.GetByIdAsync(session.TenantId);
            Assert.NotNull(tenant);
            tenant!.MixedProductLinesAllowed = false;
            await _sessionRepo.SaveChangesAsync();

            await _sessionRepo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity
            {
                ProductLineKey = "apex",
                Location = "Upstairs"
            });
            await _sessionRepo.SaveChangesAsync();

            var updated = await _sessionRepo.GetByIdAsync(session.Id);
            var apexItemId = updated!.Items.Single(i => i.ProductLineKey == "apex").Id;
            var payload = BuildValidCompletionPayload(
                session.Id,
                apexItemId,
                productLineKey: "apex",
                productLineName: "Apex");

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("mixed product lines", JsonSerializer.Serialize(badRequest.Value), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Complete_WithSectionWidthAboveStyleMaximum_ReturnsValidationError()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var itemId = Assert.Single((await _sessionRepo.GetByIdAsync(session.Id))!.Items).Id;
            var payload = BuildValidCompletionPayload(
                session.Id,
                itemId,
                styleName: "Casement");

            var payloadObject = JsonNode.Parse(payload.GetRawText())!.AsObject();
            var sections = payloadObject["sections"]!.AsArray();
            var section = sections[0]!.AsObject();
            section["width"] = JsonSerializer.SerializeToNode(new { sign = 1, whole = 38, numerator = 0, denominator = 1 });
            section["height"] = JsonSerializer.SerializeToNode(new { sign = 1, whole = 36, numerator = 0, denominator = 1 });
            payloadObject["frameWidth"] = JsonSerializer.SerializeToNode(new { sign = 1, whole = 38, numerator = 0, denominator = 1 });
            payloadObject["osmWidth"] = JsonSerializer.SerializeToNode(new { sign = 1, whole = 38, numerator = 0, denominator = 1 });

            var result = await _controller.Complete(session.Id.ToString(), JsonSerializer.SerializeToElement(payloadObject));

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("sections[0].width", JsonSerializer.Serialize(badRequest.Value));
        }

        [Fact]
        public async Task Complete_WithSubmittedTwoSectionPayload_PricesBrickmouldFromTopLevelSizing()
        {
            _fakeReader.UseRealFiles = true;
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var itemId = Assert.Single((await _sessionRepo.GetByIdAsync(session.Id))!.Items).Id;
            var payload = LoadSubmittedPricingPayload(session.Id, itemId);

            var result = await _controller.Complete(session.Id.ToString(), payload);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(447.75m, GetDecimalProperty(okResult.Value!, "authoritativePrice"));
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        /// <summary>
        /// Test double for ITemplateReader. Returns predictable strings keyed by filename.
        /// </summary>
        internal class FakeTemplateReader : ITemplateReader
        {
            public bool UseRealFiles { get; set; }

            public async Task<string> ReadTemplateAsync(string filename)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", filename);
                return UseRealFiles && File.Exists(path)
                    ? await File.ReadAllTextAsync(path)
                    : $"{filename} content";
            }
        }

        private static PriceInfoRoot LoadPriceInfo()
        {
            var appDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData");
            var path = Path.Combine(appDataPath, "priceInfo.json");
            var json = File.ReadAllText(path);
            var root = JsonSerializer.Deserialize<PriceInfoRoot>(
                json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            PricingGridAligner.AlignToCatalogTemplates(root, appDataPath);
            return root;
        }

        private static decimal GetDecimalProperty(object value, string propertyName)
        {
            var prop = value.GetType().GetProperty(propertyName);
            Assert.NotNull(prop);
            return Convert.ToDecimal(prop!.GetValue(value));
        }

        private static string GetStringProperty(object value, string propertyName)
        {
            var prop = value.GetType().GetProperty(propertyName);
            Assert.NotNull(prop);
            return Convert.ToString(prop!.GetValue(value))!;
        }

        private static JsonElement BuildValidCompletionPayload(
            Guid sessionId,
            Guid itemId,
            string productLineKey = "energysaver-2500",
            string productLineName = "EnergySaver 2500",
            int frameWidthWhole = 24,
            int frameHeightWhole = 36,
            int sectionWidthWhole = 24,
            int sectionHeightWhole = 36,
            string styleName = "Casement",
            string frameColorName = "White")
        {
            return JsonSerializer.SerializeToElement(new
            {
                id = itemId,
                orderId = sessionId,
                location = "Bedroom Left",
                lineItemNumber = "101",
                meetsEgress = true,
                productLine = new
                {
                    key = productLineKey,
                    name = productLineName,
                    manufacturerName = "All Weather Windows"
                },
                frameWidth = Measurement(frameWidthWhole),
                frameHeight = Measurement(frameHeightWhole),
                roWidth = Measurement(frameWidthWhole + 1),
                roHeight = Measurement(frameHeightWhole + 1),
                osmWidth = Measurement(frameWidthWhole),
                osmHeight = Measurement(frameHeightWhole),
                frameColor = new { name = frameColorName },
                brickmouldStyle = new { name = "None" },
                paneConfiguration = new { name = "Dual" },
                sections = new[]
                {
                    new
                    {
                        width = Measurement(sectionWidthWhole),
                        height = Measurement(sectionHeightWhole),
                        style = new { name = styleName },
                        grillePattern = new { name = "None" },
                        sdlPattern = new { name = "None" },
                        crank = new { name = "None" }
                    }
                }
            });
        }

        private static object Measurement(int whole)
            => new { sign = 1, whole, numerator = 0, denominator = 1 };

        private static JsonElement LoadSubmittedPricingPayload(Guid sessionId, Guid itemId)
        {
            var path = Path.Combine(
                FindSolutionRoot(),
                "WindowConfigurator.Tests",
                "Pricing",
                "fixtures",
                "submitted-44775-payload.json");

            var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            node["id"] = itemId.ToString();
            node["orderId"] = sessionId.ToString();
            return JsonSerializer.SerializeToElement(node);
        }

        private static string FindSolutionRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "WindowConfigurator.sln")))
                    return current.FullName;

                current = current.Parent;
            }

            return Directory.GetCurrentDirectory();
        }
    }
}
