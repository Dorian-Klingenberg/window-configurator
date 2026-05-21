using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WindowConfigurator.Controllers;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Pricing;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Controllers
{
    public class OrderItemControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly IQuoteSessionRepository _sessionRepo;
        private readonly ICatalogService _catalog;
        private readonly IPricingService _pricingService;
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
            _catalog = new CatalogService();
            _pricingService = new PricingService(LoadPriceInfo());
            _fakeReader = new FakeTemplateReader();
            _controller = new OrderItemController(_sessionRepo, _catalog, _fakeReader, _pricingService);
        }

        private async Task<QuoteSessionEntity> SeedSessionWithProductLine(string productLineKey)
        {
            var tenant = new TenantEntity { Name = "Test Dealer", ApiKey = Guid.NewGuid().ToString() };
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
        public async Task Complete_WithExistingSession_ComputesAuthoritativePriceAndMarksSessionComplete()
        {
            var session = await SeedSessionWithProductLine("energysaver-2500");
            var persisted = await _sessionRepo.GetByIdAsync(session.Id);
            var existingItemId = Assert.Single(persisted!.Items).Id;

            var payload = JsonSerializer.SerializeToElement(new
            {
                id = existingItemId,
                orderId = session.Id,
                location = "Bedroom Left",
                lineItemNumber = "101",
                meetsEgress = true,
                productLine = new
                {
                    key = "energysaver-2500",
                    name = "EnergySaver 2500",
                    manufacturerName = "All Weather Windows"
                },
                frameWidth = new { sign = 1, whole = 24, numerator = 0, denominator = 1 },
                frameHeight = new { sign = 1, whole = 36, numerator = 0, denominator = 1 },
                roWidth = new { sign = 1, whole = 25, numerator = 0, denominator = 1 },
                roHeight = new { sign = 1, whole = 37, numerator = 0, denominator = 1 },
                osmWidth = new { sign = 1, whole = 24, numerator = 0, denominator = 1 },
                osmHeight = new { sign = 1, whole = 36, numerator = 0, denominator = 1 },
                frameColor = new { name = "White" },
                brickmouldStyle = new { name = "None" },
                paneConfiguration = new { name = "Dual" },
                sections = new[]
                {
                    new
                    {
                        width = new { sign = 1, whole = 24, numerator = 0, denominator = 1 },
                        height = new { sign = 1, whole = 36, numerator = 0, denominator = 1 },
                        style = new { name = "Casement" },
                        grillePattern = new { name = "None" },
                        sdlPattern = new { name = "None" },
                        crank = new { name = "None" }
                    }
                }
            });

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
            public Task<string> ReadTemplateAsync(string filename)
                => Task.FromResult($"{filename} content");
        }

        private static PriceInfoRoot LoadPriceInfo()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WindowConfigurator", "AppData", "priceInfo.json");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PriceInfoRoot>(
                json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
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
    }
}
