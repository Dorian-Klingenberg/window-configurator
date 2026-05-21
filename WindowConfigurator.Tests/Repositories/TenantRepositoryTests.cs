using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Tests.Repositories
{
    public class TenantRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _repo;

        public TenantRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _repo = new EfTenantRepository(_context);
        }

        [Fact]
        public async Task AddAsync_PersistsTenant()
        {
            var tenant = new TenantEntity { Name = "Acme Windows", ApiKey = "acme-key" };
            await _repo.AddAsync(tenant);
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(tenant.Id);
            Assert.NotNull(loaded);
            Assert.Equal("Acme Windows", loaded.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByApiKeyAsync_ReturnsCorrectTenant()
        {
            var tenant = new TenantEntity { Name = "Acme Windows", ApiKey = "unique-api-key" };
            await _repo.AddAsync(tenant);
            await _repo.SaveChangesAsync();

            var loaded = await _repo.GetByApiKeyAsync("unique-api-key");
            Assert.NotNull(loaded);
            Assert.Equal(tenant.Id, loaded.Id);
        }

        [Fact]
        public async Task GetByApiKeyAsync_ReturnsNull_WhenKeyNotFound()
        {
            var result = await _repo.GetByApiKeyAsync("nonexistent-key");
            Assert.Null(result);
        }

        [Fact]
        public async Task AllowedProductLineKeys_RoundTrips_Correctly()
        {
            var tenant = new TenantEntity
            {
                Name = "Multi-Line Dealer",
                ApiKey = "multi-key",
                AllowedProductLineKeys = new List<string> { "energysaver-2500", "apex", "carriage" }
            };
            await _repo.AddAsync(tenant);
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(tenant.Id);
            Assert.NotNull(loaded);
            Assert.Equal(3, loaded.AllowedProductLineKeys.Count);
            Assert.Contains("energysaver-2500", loaded.AllowedProductLineKeys);
            Assert.Contains("apex", loaded.AllowedProductLineKeys);
            Assert.Contains("carriage", loaded.AllowedProductLineKeys);
        }

        [Fact]
        public async Task AllowedProductLineKeys_RoundTrips_WhenEmpty()
        {
            var tenant = new TenantEntity { Name = "No Lines Dealer", ApiKey = "no-lines-key" };
            await _repo.AddAsync(tenant);
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(tenant.Id);
            Assert.NotNull(loaded);
            Assert.Empty(loaded.AllowedProductLineKeys);
        }

        [Fact]
        public async Task Branding_RoundTrips_Correctly()
        {
            var tenant = new TenantEntity
            {
                Name = "Branded Dealer",
                ApiKey = "branded-key",
                Branding = new TenantBrandingConfig
                {
                    PrimaryColor = "#FF5733",
                    AccentColor = "#C70039",
                    LogoUrl = "https://example.com/logo.png"
                }
            };
            await _repo.AddAsync(tenant);
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(tenant.Id);
            Assert.NotNull(loaded);
            Assert.Equal("#FF5733", loaded.Branding.PrimaryColor);
            Assert.Equal("#C70039", loaded.Branding.AccentColor);
            Assert.Equal("https://example.com/logo.png", loaded.Branding.LogoUrl);
        }

        [Fact]
        public async Task MixedProductLinesAllowed_DefaultsToFalse_AfterRoundTrip()
        {
            var tenant = new TenantEntity { Name = "Default Policy Dealer", ApiKey = "default-policy-key" };
            await _repo.AddAsync(tenant);
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(tenant.Id);
            Assert.NotNull(loaded);
            Assert.False(loaded.MixedProductLinesAllowed);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
