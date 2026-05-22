using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Controllers.Api.V1;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Webhooks
{
    /// <summary>
    /// E2E harness: real controller wired to the real dispatcher wired to a stub HTTP receiver.
    /// Proves the full delivery loop — session submit → HTTP POST → payload shape → attempt persisted.
    /// No fakes in the dispatch path; only the outbound HTTP transport is intercepted.
    /// </summary>
    public class WebhookE2EHarnessTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;
        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly IWebhookDeliveryAttemptRepository _deliveryAttemptRepository;

        public WebhookE2EHarnessTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();

            _tenantRepository = new EfTenantRepository(_context);
            _sessionRepository = new EfQuoteSessionRepository(_context);
            _deliveryAttemptRepository = new EfWebhookDeliveryAttemptRepository(_context);
        }

        [Fact]
        public async Task Complete_WithRealDispatcher_DeliversFullPayloadToStubReceiverAndPersistsAttempt()
        {
            var stub = new StubWebhookReceiver(HttpStatusCode.OK);
            var controller = BuildController(stub);

            var tenant = await SeedTenantAsync(controller, "https://stub.test/webhook");
            var session = await SeedCompletedSessionAsync(tenant.Id);

            var result = await controller.Complete(session.Id, new CompleteQuoteSessionRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<CompleteQuoteSessionResponse>(ok.Value);
            Assert.Equal("Submitted", response.Status);
            Assert.Equal("delivered", response.WebhookDispatchStatus);

            // Stub received exactly one call
            Assert.Equal(1, stub.CallCount);

            // Deserialize the captured payload and verify its shape
            var payload = stub.DeserializeLastBody<TestPayload>();
            Assert.NotNull(payload);
            Assert.Equal("quote.completed", payload!.EventType);
            Assert.Equal(session.Id, payload.Session.Id);
            Assert.Equal(tenant.Id, payload.Session.TenantId);
            Assert.Equal("CRM-E2E-001", payload.Session.ExternalReferenceId);
            Assert.Single(payload.OrderGroups);

            var group = payload.OrderGroups[0];
            Assert.Equal("energysaver-2500", group.ProductLineKey);
            Assert.Single(group.Items);

            var item = group.Items[0];
            Assert.Equal("Living Room", item.Location);
            Assert.Equal(425.50m, item.AuthoritativePrice);
            Assert.Equal(24m, item.Measurements.Frame.WidthInches);
            Assert.Equal(36m, item.Measurements.Frame.HeightInches);
            Assert.Null(item.Measurements.RoughOpening);

            // Delivery attempt persisted as Delivered
            var attempt = Assert.Single(_context.WebhookDeliveryAttempts);
            Assert.Equal("Delivered", attempt.Status);
            Assert.Equal(200, attempt.StatusCode);
            Assert.Equal(session.Id, attempt.QuoteSessionId);
            Assert.Equal(tenant.Id, attempt.TenantId);
        }

        [Fact]
        public async Task Complete_WhenStubReceiverReturnsServerError_PersistsFailedAttemptWithRetryScheduled()
        {
            var stub = new StubWebhookReceiver(HttpStatusCode.ServiceUnavailable);
            var controller = BuildController(stub);

            var tenant = await SeedTenantAsync(controller, "https://stub.test/webhook");
            var session = await SeedCompletedSessionAsync(tenant.Id);

            var result = await controller.Complete(session.Id, new CompleteQuoteSessionRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<CompleteQuoteSessionResponse>(ok.Value);
            Assert.Equal("failed", response.WebhookDispatchStatus);
            Assert.Equal(503, response.WebhookStatusCode);

            // Stub received the call despite the error response
            Assert.Equal(1, stub.CallCount);

            // Failed attempt persisted with retry metadata
            var attempt = Assert.Single(_context.WebhookDeliveryAttempts);
            Assert.Equal("Failed", attempt.Status);
            Assert.Equal(503, attempt.StatusCode);
            Assert.Equal(1, attempt.AttemptCount);
            Assert.NotNull(attempt.NextRetryAtUtc);
            Assert.True(attempt.NextRetryAtUtc > DateTime.UtcNow);
        }

        private QuoteSessionsController BuildController(StubWebhookReceiver stub)
        {
            var httpClient = new HttpClient(stub);
            var dispatcher = new QuoteCompletionWebhookDispatcher(httpClient);

            var controller = new QuoteSessionsController(
                _sessionRepository,
                _tenantRepository,
                new CatalogService(),
                dispatcher,
                _deliveryAttemptRepository);

            SetAuthenticatedTenant(controller, Guid.Empty);
            return controller;
        }

        private async Task<TenantEntity> SeedTenantAsync(QuoteSessionsController controller, string callbackUrl)
        {
            var tenant = new TenantEntity
            {
                Name = "E2E Test Tenant",
                ApiKey = Guid.NewGuid().ToString("N"),
                AllowedProductLineKeys = ["energysaver-2500"],
                WebhookCallbackUrl = callbackUrl
            };
            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();
            SetAuthenticatedTenant(controller, tenant.Id);
            return tenant;
        }

        private async Task<QuoteSessionEntity> SeedCompletedSessionAsync(Guid tenantId)
        {
            var session = new QuoteSessionEntity
            {
                TenantId = tenantId,
                Status = QuoteSessionStatus.Completed,
                DefaultProductLineKey = "energysaver-2500",
                ExternalReferenceId = "CRM-E2E-001",
                CompletedAt = DateTime.UtcNow
            };
            session.Items.Add(new ConfiguredWindowItemEntity
            {
                QuoteSessionId = session.Id,
                Status = ConfiguredWindowItemStatus.Completed,
                ProductLineKey = "energysaver-2500",
                Location = "Living Room",
                LineItemNumber = 1,
                SectionCount = 1,
                AuthoritativePrice = 425.50m,
                FrameWidthDecimal = 24m,
                FrameHeightDecimal = 36m,
                OutsideMeasureWidthDecimal = 24m,
                OutsideMeasureHeightDecimal = 36m,
                ConfigurationJson = "{\"sizingType\":\"Frame\"}"
            });
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();
            return session;
        }

        private static void SetAuthenticatedTenant(QuoteSessionsController controller, Guid tenantId)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items[ApiKeyAuthorizeFilter.TenantIdItemKey] = tenantId;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private class StubWebhookReceiver : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private string _lastBody = string.Empty;

            public int CallCount { get; private set; }

            public StubWebhookReceiver(HttpStatusCode statusCode) => _statusCode = statusCode;

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                CallCount++;
                _lastBody = request.Content is null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            }

            public T? DeserializeLastBody<T>()
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<T>(_lastBody, options);
            }
        }

        // Local DTOs for deserializing the captured payload body in assertions.
        // These mirror QuoteCompletedPayload without coupling the test to internal types.
        private class TestPayload
        {
            public string EventType { get; set; } = string.Empty;
            public TestSession Session { get; set; } = new();
            public List<TestOrderGroup> OrderGroups { get; set; } = new();
        }

        private class TestSession
        {
            public Guid Id { get; set; }
            public Guid TenantId { get; set; }
            public string? ExternalReferenceId { get; set; }
        }

        private class TestOrderGroup
        {
            public string ProductLineKey { get; set; } = string.Empty;
            public List<TestItem> Items { get; set; } = new();
        }

        private class TestItem
        {
            public string Location { get; set; } = string.Empty;
            public decimal? AuthoritativePrice { get; set; }
            public TestMeasurements Measurements { get; set; } = new();
        }

        private class TestMeasurements
        {
            public TestDimension Frame { get; set; } = new();
            public TestDimension? RoughOpening { get; set; }
        }

        private class TestDimension
        {
            public decimal? WidthInches { get; set; }
            public decimal? HeightInches { get; set; }
        }
    }
}
