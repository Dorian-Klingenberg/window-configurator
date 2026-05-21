using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Tests.Infrastructure;

namespace WindowConfigurator.Tests.Repositories
{
    public class QuoteSessionRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly IQuoteSessionRepository _repo;

        public QuoteSessionRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _repo = new EfQuoteSessionRepository(_context);
        }

        private TenantEntity SeedTenant(string apiKey = "")
        {
            var tenant = new TenantEntity
            {
                Name = "Test Dealer",
                ApiKey = string.IsNullOrEmpty(apiKey) ? Guid.NewGuid().ToString() : apiKey
            };
            _context.Tenants.Add(tenant);
            _context.SaveChanges();
            return tenant;
        }

        [Fact]
        public async Task AddAsync_PersistsSession()
        {
            var tenant = SeedTenant();
            var session = new QuoteSessionEntity { TenantId = tenant.Id, ExternalReferenceId = "CRM-001" };

            await _repo.AddAsync(session);
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(session.Id);
            Assert.NotNull(loaded);
            Assert.Equal("CRM-001", loaded.ExternalReferenceId);
            Assert.Equal(QuoteSessionStatus.Draft, loaded.Status);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_IncludesItems()
        {
            var tenant = SeedTenant();
            var session = new QuoteSessionEntity { TenantId = tenant.Id };
            await _repo.AddAsync(session);
            await _repo.SaveChangesAsync();

            await _repo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity
            {
                Location = "Master Bedroom Left",
                ProductLineKey = "energysaver-2500"
            });
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(session.Id);
            Assert.NotNull(loaded);
            Assert.Single(loaded.Items);
            Assert.Equal("Master Bedroom Left", loaded.Items[0].Location);
        }

        [Fact]
        public async Task AddItemAsync_SetsQuoteSessionId()
        {
            var tenant = SeedTenant();
            var session = new QuoteSessionEntity { TenantId = tenant.Id };
            await _repo.AddAsync(session);
            await _repo.SaveChangesAsync();

            var item = new ConfiguredWindowItemEntity { ProductLineKey = "apex" };
            await _repo.AddItemAsync(session.Id, item);
            await _repo.SaveChangesAsync();

            Assert.Equal(session.Id, item.QuoteSessionId);
        }

        [Fact]
        public async Task ListByTenantAsync_ReturnsOnlySessionsForTenant()
        {
            var tenantA = SeedTenant();
            var tenantB = SeedTenant();
            await _repo.AddAsync(new QuoteSessionEntity { TenantId = tenantA.Id });
            await _repo.AddAsync(new QuoteSessionEntity { TenantId = tenantA.Id });
            await _repo.AddAsync(new QuoteSessionEntity { TenantId = tenantB.Id });
            await _repo.SaveChangesAsync();

            var results = await _repo.ListByTenantAsync(tenantA.Id);
            Assert.Equal(2, results.Count);
            Assert.All(results, s => Assert.Equal(tenantA.Id, s.TenantId));
        }

        [Fact]
        public async Task ListByTenantAsync_IncludesItemsOnEachSession()
        {
            var tenant = SeedTenant();
            var session = new QuoteSessionEntity { TenantId = tenant.Id };
            await _repo.AddAsync(session);
            await _repo.SaveChangesAsync();
            await _repo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity { ProductLineKey = "apex" });
            await _repo.SaveChangesAsync();

            var results = await _repo.ListByTenantAsync(tenant.Id);
            Assert.Single(results[0].Items);
        }

        [Fact]
        public async Task DeletingSession_CascadesToItems()
        {
            var tenant = SeedTenant();
            var session = new QuoteSessionEntity { TenantId = tenant.Id };
            await _repo.AddAsync(session);
            await _repo.SaveChangesAsync();

            await _repo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity { ProductLineKey = "carriage" });
            await _repo.SaveChangesAsync();

            _context.QuoteSessions.Remove(session);
            await _repo.SaveChangesAsync();

            var itemCount = _context.ConfiguredWindowItems.Count(i => i.QuoteSessionId == session.Id);
            Assert.Equal(0, itemCount);
        }

        [Fact]
        public async Task Session_CanHold_MultipleItems()
        {
            var tenant = SeedTenant();
            var session = new QuoteSessionEntity { TenantId = tenant.Id };
            await _repo.AddAsync(session);
            await _repo.SaveChangesAsync();

            await _repo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity { Location = "Kitchen", ProductLineKey = "energysaver-2500" });
            await _repo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity { Location = "Living Room", ProductLineKey = "apex" });
            await _repo.AddItemAsync(session.Id, new ConfiguredWindowItemEntity { Location = "Master Bedroom", ProductLineKey = "carriage" });
            await _repo.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var loaded = await _repo.GetByIdAsync(session.Id);
            Assert.NotNull(loaded);
            Assert.Equal(3, loaded.Items.Count);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
