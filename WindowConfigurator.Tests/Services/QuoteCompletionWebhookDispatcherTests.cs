using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Services
{
    public class QuoteCompletionWebhookDispatcherTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;

        public QuoteCompletionWebhookDispatcherTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task DispatchQuoteCompletedAsync_WhenEndpointReturnsOk_ReturnsSuccess()
        {
            var handler = new RecordingHandler(HttpStatusCode.OK);
            var client = new HttpClient(handler);
            var dispatcher = new QuoteCompletionWebhookDispatcher(client);
            var tenant = new TenantEntity { WebhookCallbackUrl = "https://example.test/webhook" };
            var session = BuildCompletedSession(tenant.Id);

            var result = await dispatcher.DispatchQuoteCompletedAsync(session, tenant, new CatalogService());

            Assert.True(result.Succeeded);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("\"eventType\":\"quote.completed\"", handler.LastBody);
        }

        [Fact]
        public async Task DispatchQuoteCompletedAsync_WhenEndpointReturnsError_ReturnsFailure()
        {
            var handler = new RecordingHandler(HttpStatusCode.BadRequest);
            var client = new HttpClient(handler);
            var dispatcher = new QuoteCompletionWebhookDispatcher(client);
            var tenant = new TenantEntity { WebhookCallbackUrl = "https://example.test/webhook" };
            var session = BuildCompletedSession(tenant.Id);

            var result = await dispatcher.DispatchQuoteCompletedAsync(session, tenant, new CatalogService());

            Assert.False(result.Succeeded);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(result.Error);
        }

        private static QuoteSessionEntity BuildCompletedSession(Guid tenantId)
        {
            var session = new QuoteSessionEntity
            {
                TenantId = tenantId,
                Status = QuoteSessionStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                ExternalReferenceId = "CRM-100"
            };

            session.Items.Add(new ConfiguredWindowItemEntity
            {
                QuoteSessionId = session.Id,
                Status = ConfiguredWindowItemStatus.Completed,
                ProductLineKey = "energysaver-2500",
                Location = "Kitchen",
                LineItemNumber = 1,
                SectionCount = 1,
                AuthoritativePrice = 300m,
                FrameWidthDecimal = 24m,
                FrameHeightDecimal = 36m,
                OutsideMeasureWidthDecimal = 24m,
                OutsideMeasureHeightDecimal = 36m,
                ConfigurationJson = "{\"sample\":true}"
            });

            return session;
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private class RecordingHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            public string LastBody { get; private set; } = string.Empty;

            public RecordingHandler(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastBody = request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
