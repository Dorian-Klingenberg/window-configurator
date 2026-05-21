using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Services
{
    public class WebhookRetryProcessorTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly FakeDispatcher _dispatcher;
        private readonly WebhookRetryProcessor _processor;

        public WebhookRetryProcessorTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();

            _sessionRepository = new EfQuoteSessionRepository(_context);
            _tenantRepository = new EfTenantRepository(_context);
            _dispatcher = new FakeDispatcher();
            _processor = new WebhookRetryProcessor(
                _context,
                _dispatcher,
                _sessionRepository,
                _tenantRepository,
                new CatalogService());
        }

        [Fact]
        public async Task ProcessDueRetriesAsync_WhenDispatchSucceeds_MarksAttemptDelivered()
        {
            var (tenant, session, attempt) = await SeedFailedAttemptAsync();
            _dispatcher.Next = new QuoteCompletionWebhookDispatchResult { Succeeded = true, StatusCode = 200 };

            var processed = await _processor.ProcessDueRetriesAsync();

            Assert.Equal(1, processed);
            var reloaded = await _context.WebhookDeliveryAttempts.SingleAsync(x => x.Id == attempt.Id);
            Assert.Equal("Delivered", reloaded.Status);
            Assert.Equal(2, reloaded.AttemptCount);
            Assert.Null(reloaded.NextRetryAtUtc);
        }

        [Fact]
        public async Task ProcessDueRetriesAsync_WhenDispatchFails_SchedulesNextRetry()
        {
            var (tenant, session, attempt) = await SeedFailedAttemptAsync();
            _dispatcher.Next = new QuoteCompletionWebhookDispatchResult { Succeeded = false, StatusCode = 502, Error = "bad gateway" };

            var processed = await _processor.ProcessDueRetriesAsync();

            Assert.Equal(1, processed);
            var reloaded = await _context.WebhookDeliveryAttempts.SingleAsync(x => x.Id == attempt.Id);
            Assert.Equal("Failed", reloaded.Status);
            Assert.Equal(2, reloaded.AttemptCount);
            Assert.Equal(502, reloaded.StatusCode);
            Assert.NotNull(reloaded.NextRetryAtUtc);
        }

        private async Task<(TenantEntity tenant, QuoteSessionEntity session, WebhookDeliveryAttemptEntity attempt)> SeedFailedAttemptAsync()
        {
            var tenant = new TenantEntity
            {
                Name = "Retry Tenant",
                ApiKey = Guid.NewGuid().ToString("N"),
                WebhookCallbackUrl = "https://example.test/webhook",
                AllowedProductLineKeys = ["energysaver-2500"]
            };
            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();

            var session = new QuoteSessionEntity
            {
                TenantId = tenant.Id,
                Status = QuoteSessionStatus.Submitted,
                DefaultProductLineKey = "energysaver-2500"
            };
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.AddItemAsync(session.Id, new ConfiguredWindowItemEntity
            {
                ProductLineKey = "energysaver-2500",
                Status = ConfiguredWindowItemStatus.Completed,
                Location = "Kitchen"
            });
            await _sessionRepository.SaveChangesAsync();

            var attempt = new WebhookDeliveryAttemptEntity
            {
                QuoteSessionId = session.Id,
                TenantId = tenant.Id,
                Status = "Failed",
                AttemptCount = 1,
                NextRetryAtUtc = DateTime.UtcNow.AddMinutes(-1)
            };
            await _context.WebhookDeliveryAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();

            return (tenant, session, attempt);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private class FakeDispatcher : IQuoteCompletionWebhookDispatcher
        {
            public QuoteCompletionWebhookDispatchResult Next { get; set; } = new() { Succeeded = true, StatusCode = 200 };

            public Task<QuoteCompletionWebhookDispatchResult> DispatchQuoteCompletedAsync(
                QuoteSessionEntity session,
                TenantEntity tenant,
                ICatalogService catalog,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Next);
            }
        }
    }
}
