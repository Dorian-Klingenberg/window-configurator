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

namespace WindowConfigurator.Tests.Controllers
{
    public class TenantApiKeyControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;

        public TenantApiKeyControllerTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _tenantRepository = new EfTenantRepository(_context);
        }

        [Fact]
        public async Task Rotate_WhenAuthenticated_GeneratesNewKeyAndReturnsIt()
        {
            var tenant = await SeedTenantAsync("original-key");
            var controller = BuildController(tenant.Id);

            var result = await controller.Rotate(tenant.Id, new RotateApiKeyRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<RotateApiKeyResponse>(ok.Value);
            Assert.Equal(tenant.Id, response.TenantId);
            Assert.NotEmpty(response.ApiKey);
            Assert.NotEqual("original-key", response.ApiKey);
            Assert.Null(response.ExpiresAtUtc);
        }

        [Fact]
        public async Task Rotate_WithExpiresAtUtc_SetsExpiryOnNewKey()
        {
            var tenant = await SeedTenantAsync("original-key");
            var controller = BuildController(tenant.Id);
            var expiry = DateTime.UtcNow.AddDays(90);

            var result = await controller.Rotate(tenant.Id, new RotateApiKeyRequest { ExpiresAtUtc = expiry });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<RotateApiKeyResponse>(ok.Value);
            Assert.NotNull(response.ExpiresAtUtc);
            Assert.True(response.ExpiresAtUtc!.Value >= expiry.AddSeconds(-1));

            var persisted = await _tenantRepository.GetByIdAsync(tenant.Id);
            Assert.NotNull(persisted!.ApiKeyExpiresAtUtc);
            Assert.Null(persisted.ApiKeyRevokedAt);
        }

        [Fact]
        public async Task Rotate_ClearsAnyPreviousRevocationOrExpiry()
        {
            var tenant = await SeedTenantAsync("old-key");
            tenant.ApiKeyRevokedAt = DateTime.UtcNow.AddHours(-1);
            tenant.ApiKeyExpiresAtUtc = DateTime.UtcNow.AddHours(-1);
            await _tenantRepository.SaveChangesAsync();
            var controller = BuildController(tenant.Id);

            var result = await controller.Rotate(tenant.Id, new RotateApiKeyRequest());

            Assert.IsType<OkObjectResult>(result.Result);
            var persisted = await _tenantRepository.GetByIdAsync(tenant.Id);
            Assert.Null(persisted!.ApiKeyRevokedAt);
            Assert.Null(persisted.ApiKeyExpiresAtUtc);
        }

        [Fact]
        public async Task Revoke_WhenAuthenticated_SetsRevokedAtAndReturnsNoContent()
        {
            var tenant = await SeedTenantAsync("live-key");
            var controller = BuildController(tenant.Id);

            var result = await controller.Revoke(tenant.Id);

            Assert.IsType<NoContentResult>(result);
            var persisted = await _tenantRepository.GetByIdAsync(tenant.Id);
            Assert.NotNull(persisted!.ApiKeyRevokedAt);
            Assert.True(persisted.ApiKeyRevokedAt!.Value <= DateTime.UtcNow);
        }

        [Fact]
        public async Task Revoke_WhenTenantScopeMismatch_ReturnsForbidden()
        {
            var tenant = await SeedTenantAsync("live-key");
            var controller = BuildController(Guid.NewGuid()); // different tenant authenticated

            var result = await controller.Revoke(tenant.Id);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, obj.StatusCode);
        }

        [Fact]
        public async Task Rotate_WhenTenantScopeMismatch_ReturnsForbidden()
        {
            var tenant = await SeedTenantAsync("live-key");
            var controller = BuildController(Guid.NewGuid());

            var result = await controller.Rotate(tenant.Id, new RotateApiKeyRequest());

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, obj.StatusCode);
        }

        private async Task<TenantEntity> SeedTenantAsync(string apiKey)
        {
            var tenant = new TenantEntity
            {
                Name = "Key Test Tenant",
                ApiKey = apiKey,
                AllowedProductLineKeys = ["energysaver-2500"]
            };
            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();
            return tenant;
        }

        private TenantApiKeyController BuildController(Guid authenticatedTenantId)
        {
            var controller = new TenantApiKeyController(_tenantRepository);
            var httpContext = new DefaultHttpContext();
            httpContext.Items[ApiKeyAuthorizeFilter.TenantIdItemKey] = authenticatedTenantId;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
