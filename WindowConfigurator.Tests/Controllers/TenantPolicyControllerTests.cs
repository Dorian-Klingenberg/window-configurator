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
    public class TenantPolicyControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;
        private readonly TenantPolicyController _controller;

        public TenantPolicyControllerTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _tenantRepository = new EfTenantRepository(_context);
            _controller = new TenantPolicyController(_tenantRepository);
        }

        [Fact]
        public async Task UpdatePolicy_PersistsAllowedProductLinesAndBranding()
        {
            var tenant = await SeedTenantAsync();

            var result = await _controller.UpdatePolicy(tenant.Id, new TenantPolicySettingsRequest
            {
                AllowedProductLineKeys = ["apex", "carriage"],
                MixedProductLinesAllowed = true,
                BrandingPrimaryColor = "#112233"
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TenantPolicySettingsResponse>(ok.Value);
            Assert.Contains("apex", response.AllowedProductLineKeys);
            Assert.True(response.MixedProductLinesAllowed);
            Assert.Equal("#112233", response.BrandingPrimaryColor);
        }

        private async Task<TenantEntity> SeedTenantAsync()
        {
            var tenant = new TenantEntity
            {
                Name = "Tenant",
                ApiKey = Guid.NewGuid().ToString("N"),
                WebhookCallbackUrl = "https://hook"
            };
            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();

            var http = new DefaultHttpContext();
            http.Items[ApiKeyAuthorizeFilter.TenantIdItemKey] = tenant.Id;
            _controller.ControllerContext = new ControllerContext { HttpContext = http };
            return tenant;
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
