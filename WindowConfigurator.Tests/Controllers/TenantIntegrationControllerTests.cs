using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WindowConfigurator.Controllers.Api.V1;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Tests.Controllers
{
    public class TenantIntegrationControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;
        private readonly TenantIntegrationController _controller;

        public TenantIntegrationControllerTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _tenantRepository = new EfTenantRepository(_context);
            _controller = new TenantIntegrationController(_tenantRepository);
        }

        [Fact]
        public async Task UpdateIntegration_WithAbsoluteUrl_PersistsCallbackUrl()
        {
            var tenant = await SeedTenantAsync();

            var result = await _controller.UpdateIntegration(tenant.Id, new TenantIntegrationSettingsRequest
            {
                WebhookCallbackUrl = "https://hooks.example.com/quote"
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TenantIntegrationSettingsResponse>(ok.Value);
            Assert.Equal("https://hooks.example.com/quote", response.WebhookCallbackUrl);
        }

        [Fact]
        public async Task GetIntegration_WithMismatchedTenantScope_ReturnsForbidden()
        {
            var tenant = await SeedTenantAsync();
            SetAuthenticatedTenant(Guid.NewGuid());

            var result = await _controller.GetIntegration(tenant.Id);

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task UpdateIntegration_WithRelativeUrl_ReturnsValidationError()
        {
            var tenant = await SeedTenantAsync();

            var result = await _controller.UpdateIntegration(tenant.Id, new TenantIntegrationSettingsRequest
            {
                WebhookCallbackUrl = "/relative"
            });

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            var error = Assert.IsType<ApiErrorResponse>(bad.Value);
            Assert.Contains(error.ValidationErrors, e => e.Field == "webhookCallbackUrl");
        }

        private async Task<TenantEntity> SeedTenantAsync()
        {
            var tenant = new TenantEntity
            {
                Name = "Tenant",
                ApiKey = Guid.NewGuid().ToString("N"),
                WebhookCallbackUrl = "https://old.example.com/hook"
            };
            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();
            SetAuthenticatedTenant(tenant.Id);
            return tenant;
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
    }
}
