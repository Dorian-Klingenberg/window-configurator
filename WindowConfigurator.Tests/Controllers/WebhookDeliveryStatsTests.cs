using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Controllers.Api.V1;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Controllers
{
    public class WebhookDeliveryStatsTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly IWebhookDeliveryAttemptRepository _deliveryAttemptRepository;
        private readonly WebhookDeliveriesController _controller;

        public WebhookDeliveryStatsTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _deliveryAttemptRepository = new EfWebhookDeliveryAttemptRepository(_context);

            var fakeProcessor = new FakeRetryProcessor();
            _controller = new WebhookDeliveriesController(fakeProcessor, _deliveryAttemptRepository);
            SetAuthenticatedTenant(Guid.NewGuid());
        }

        [Fact]
        public async Task GetStats_WithMixedAttempts_ReturnsCorrectCounts()
        {
            await SeedAttemptAsync("Delivered");
            await SeedAttemptAsync("Delivered");
            await SeedAttemptAsync("Failed");
            await SeedAttemptAsync("Failed");
            await SeedAttemptAsync("Failed");

            var result = await _controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<WebhookDeliveryStatsResponse>(ok.Value);
            Assert.Equal(2, response.Delivered);
            Assert.Equal(3, response.Failed);
            Assert.Equal(5, response.Total);
        }

        [Fact]
        public async Task GetStats_WithNoAttempts_ReturnsZeroCounts()
        {
            var result = await _controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<WebhookDeliveryStatsResponse>(ok.Value);
            Assert.Equal(0, response.Delivered);
            Assert.Equal(0, response.Failed);
            Assert.Equal(0, response.Total);
        }

        private async Task SeedAttemptAsync(string status)
        {
            _context.WebhookDeliveryAttempts.Add(new WebhookDeliveryAttemptEntity
            {
                Status = status,
                QuoteSessionId = Guid.NewGuid(),
                TenantId = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();
        }

        private void SetAuthenticatedTenant(Guid tenantId)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items[ApiKeyAuthorizeFilter.TenantIdItemKey] = tenantId;
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private class FakeRetryProcessor : IWebhookRetryProcessor
        {
            public Task<int> ProcessDueRetriesAsync(int maxAttempts = 50, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
        }
    }
}
