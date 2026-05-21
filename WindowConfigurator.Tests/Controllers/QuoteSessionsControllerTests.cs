using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;
using WindowConfigurator.Controllers.Api.V1;
using WindowConfigurator.Controllers.Api.V1.Models;

namespace WindowConfigurator.Tests.Controllers
{
    public class QuoteSessionsControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;
        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly FakeQuoteCompletionWebhookDispatcher _webhookDispatcher;
        private readonly QuoteSessionsController _controller;

        public QuoteSessionsControllerTests()
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
            _webhookDispatcher = new FakeQuoteCompletionWebhookDispatcher();
            _controller = new QuoteSessionsController(_sessionRepository, _tenantRepository, new CatalogService(), _webhookDispatcher);
        }

        [Fact]
        public async Task Create_WhenRequestIsValid_CreatesDraftSessionAndReturnsCreated()
        {
            var tenant = await SeedTenantAsync(["energysaver-2500"]);

            var result = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = tenant.Id,
                ExternalReferenceId = "CRM-1001",
                DefaultProductLineKey = "energysaver-2500"
            });

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<QuoteSessionResponse>(created.Value);

            Assert.Equal("GetById", created.ActionName);
            Assert.Equal(tenant.Id, response.TenantId);
            Assert.Equal("CRM-1001", response.ExternalReferenceId);
            Assert.Equal("energysaver-2500", response.DefaultProductLineKey);
            Assert.Equal("Draft", response.Status);
            Assert.StartsWith("/", response.SessionUrl, StringComparison.Ordinal);

            var persisted = await _sessionRepository.GetByIdAsync(response.SessionId);
            Assert.NotNull(persisted);
            Assert.Equal(QuoteSessionStatus.Draft, persisted!.Status);
        }

        [Fact]
        public async Task Create_WhenTenantDoesNotExist_ReturnsBadRequest()
        {
            var result = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = Guid.NewGuid(),
                ExternalReferenceId = "CRM-404"
            });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
            Assert.Equal("validation_error", error.Code);
            Assert.Contains(error.ValidationErrors, e => e.Field == "tenantId");
        }

        [Fact]
        public async Task Create_WhenDefaultProductLineIsNotAllowed_ReturnsBadRequest()
        {
            var tenant = await SeedTenantAsync(["apex"]);

            var result = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = "energysaver-2500"
            });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
            Assert.Contains(error.ValidationErrors, e => e.Field == "defaultProductLineKey");
        }

        [Fact]
        public async Task GetById_WhenSessionExists_ReturnsSessionWithItems()
        {
            var tenant = await SeedTenantAsync(["energysaver-2500"]);
            var created = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = "energysaver-2500"
            });
            var createdResponse = Assert.IsType<QuoteSessionResponse>(Assert.IsType<CreatedAtActionResult>(created.Result).Value);
            await _sessionRepository.AddItemAsync(createdResponse.SessionId, new ConfiguredWindowItemEntity
            {
                ProductLineKey = "energysaver-2500",
                Location = "Kitchen",
                LineItemNumber = 1
            });
            await _sessionRepository.SaveChangesAsync();

            var result = await _controller.GetById(createdResponse.SessionId);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<QuoteSessionResponse>(ok.Value);

            Assert.Single(response.Items);
            Assert.Equal("Kitchen", response.Items[0].Location);
        }

        [Fact]
        public async Task Update_WhenDefaultProductLineIsAllowed_UpdatesSession()
        {
            var tenant = await SeedTenantAsync(["energysaver-2500", "apex"]);
            var created = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = "energysaver-2500"
            });
            var session = Assert.IsType<QuoteSessionResponse>(Assert.IsType<CreatedAtActionResult>(created.Result).Value);

            var result = await _controller.Update(session.SessionId, new UpdateQuoteSessionRequest
            {
                DefaultProductLineKey = "apex",
                ExternalReferenceId = "CRM-NEW"
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<QuoteSessionResponse>(ok.Value);
            Assert.Equal("apex", response.DefaultProductLineKey);
            Assert.Equal("CRM-NEW", response.ExternalReferenceId);
        }

        [Fact]
        public async Task AddItem_WhenRequestIsValid_PersistsDraftItem()
        {
            var tenant = await SeedTenantAsync(["energysaver-2500"]);
            var created = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = "energysaver-2500"
            });
            var session = Assert.IsType<QuoteSessionResponse>(Assert.IsType<CreatedAtActionResult>(created.Result).Value);

            var result = await _controller.AddItem(session.SessionId, new AddQuoteSessionItemRequest
            {
                Location = "Den",
                LineItemNumber = 2
            });

            var createdItem = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<QuoteSessionItemResponse>(createdItem.Value);
            Assert.Equal("Den", response.Location);
            Assert.Equal("energysaver-2500", response.ProductLineKey);
            Assert.Equal("Draft", response.Status);
        }

        [Fact]
        public async Task UpdateItem_WhenItemExists_UpdatesMutableFields()
        {
            var tenant = await SeedTenantAsync(["energysaver-2500", "apex"]);
            var created = await _controller.Create(new CreateQuoteSessionRequest
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = "energysaver-2500"
            });
            var session = Assert.IsType<QuoteSessionResponse>(Assert.IsType<CreatedAtActionResult>(created.Result).Value);
            var added = await _controller.AddItem(session.SessionId, new AddQuoteSessionItemRequest { Location = "Office" });
            var item = Assert.IsType<QuoteSessionItemResponse>(Assert.IsType<CreatedAtActionResult>(added.Result).Value);

            var result = await _controller.UpdateItem(session.SessionId, item.ItemId, new UpdateQuoteSessionItemRequest
            {
                Location = "Office Left",
                ProductLineKey = "apex",
                MeetsEgress = true
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<QuoteSessionItemResponse>(ok.Value);
            Assert.Equal("Office Left", response.Location);
            Assert.Equal("apex", response.ProductLineKey);
            Assert.True(response.MeetsEgress);
        }

        [Fact]
        public async Task Complete_WhenAllItemsCompleted_MarksSessionSubmitted()
        {
            var tenant = await SeedTenantAsync(["energysaver-2500"]);
            var session = new QuoteSessionEntity
            {
                TenantId = tenant.Id,
                Status = QuoteSessionStatus.Completed,
                DefaultProductLineKey = "energysaver-2500"
            };
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.AddItemAsync(session.Id, new ConfiguredWindowItemEntity
            {
                ProductLineKey = "energysaver-2500",
                Status = ConfiguredWindowItemStatus.Completed,
                Location = "Done"
            });
            await _sessionRepository.SaveChangesAsync();

            var result = await _controller.Complete(session.Id, new CompleteQuoteSessionRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<CompleteQuoteSessionResponse>(ok.Value);
            Assert.Equal("Submitted", response.Status);
            Assert.Equal("delivered", response.WebhookDispatchStatus);
            Assert.Equal(1, _webhookDispatcher.CallCount);
        }

        private async Task<TenantEntity> SeedTenantAsync(List<string> allowedProductLineKeys)
        {
            var tenant = new TenantEntity
            {
                Name = "Test Tenant",
                ApiKey = Guid.NewGuid().ToString("N"),
                AllowedProductLineKeys = allowedProductLineKeys
            };

            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();
            return tenant;
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private class FakeQuoteCompletionWebhookDispatcher : IQuoteCompletionWebhookDispatcher
        {
            public int CallCount { get; private set; }

            public Task<QuoteCompletionWebhookDispatchResult> DispatchQuoteCompletedAsync(
                QuoteSessionEntity session,
                TenantEntity tenant,
                ICatalogService catalog,
                CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult(new QuoteCompletionWebhookDispatchResult { Succeeded = true, StatusCode = 200 });
            }
        }
    }
}
