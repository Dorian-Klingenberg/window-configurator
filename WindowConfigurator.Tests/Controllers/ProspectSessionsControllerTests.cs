using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Controllers.Api.V1;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Tests.Controllers
{
    public class ProspectSessionsControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;
        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly ProspectSessionsController _controller;

        public ProspectSessionsControllerTests()
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
            _controller = new ProspectSessionsController(_tenantRepository, _sessionRepository);
        }

        [Fact]
        public async Task Start_AndResume_WithValidToken_ReturnsSessionUrl()
        {
            var tenant = await SeedTenantAsync();
            var started = await _controller.Start(new StartProspectSessionRequest
            {
                TenantId = tenant.Id,
                CustomerEmail = "prospect@example.com"
            });
            var startOk = Assert.IsType<OkObjectResult>(started.Result);
            var startResponse = Assert.IsType<StartProspectSessionResponse>(startOk.Value);

            var resumed = await _controller.Resume(startResponse.MagicLinkToken);
            var resumeOk = Assert.IsType<OkObjectResult>(resumed.Result);
            var resumeResponse = Assert.IsType<ResumeProspectSessionResponse>(resumeOk.Value);
            Assert.Equal(startResponse.SessionId, resumeResponse.SessionId);
            Assert.StartsWith("/", resumeResponse.SessionUrl, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Resume_WithExpiredToken_ReturnsGone()
        {
            var tenant = await SeedTenantAsync();
            var session = new QuoteSessionEntity
            {
                TenantId = tenant.Id,
                CustomerEmail = "prospect@example.com",
                MagicLinkToken = "expired",
                MagicLinkExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
            };
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            var resumed = await _controller.Resume("expired");
            var gone = Assert.IsType<ObjectResult>(resumed.Result);
            Assert.Equal(StatusCodes.Status410Gone, gone.StatusCode);
        }

        private async Task<TenantEntity> SeedTenantAsync()
        {
            var tenant = new TenantEntity
            {
                Name = "Tenant",
                ApiKey = Guid.NewGuid().ToString("N"),
                WebhookCallbackUrl = "https://hook",
                AllowedProductLineKeys = ["energysaver-2500"]
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
    }
}
