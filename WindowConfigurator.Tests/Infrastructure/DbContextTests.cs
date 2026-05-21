using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Tests.Infrastructure
{
    public class DbContextTests
    {
        internal static (WindowConfiguratorDbContext ctx, SqliteConnection conn) CreateContext()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(connection)
                .Options;
            var ctx = new WindowConfiguratorDbContext(options);
            ctx.Database.EnsureCreated();
            return (ctx, connection);
        }

        [Fact]
        public void DbContext_CanBeCreatedWithSqlite()
        {
            var (ctx, conn) = CreateContext();
            using (conn) using (ctx) { Assert.NotNull(ctx); }
        }

        [Fact]
        public void DbContext_HasTenantsDbSet()
        {
            var (ctx, conn) = CreateContext();
            using (conn) using (ctx) { Assert.NotNull(ctx.Tenants); }
        }

        [Fact]
        public void DbContext_HasQuoteSessionsDbSet()
        {
            var (ctx, conn) = CreateContext();
            using (conn) using (ctx) { Assert.NotNull(ctx.QuoteSessions); }
        }

        [Fact]
        public void DbContext_HasConfiguredWindowItemsDbSet()
        {
            var (ctx, conn) = CreateContext();
            using (conn) using (ctx) { Assert.NotNull(ctx.ConfiguredWindowItems); }
        }

        [Fact]
        public void DbContext_HasWebhookDeliveryAttemptsDbSet()
        {
            var (ctx, conn) = CreateContext();
            using (conn) using (ctx) { Assert.NotNull(ctx.WebhookDeliveryAttempts); }
        }

        [Fact]
        public void DbContext_CanSaveAndReload_Tenant()
        {
            var (ctx, conn) = CreateContext();
            using (conn)
            using (ctx)
            {
                var tenant = new TenantEntity { Name = "Test Dealer", ApiKey = "test-key" };
                ctx.Tenants.Add(tenant);
                ctx.SaveChanges();

                ctx.ChangeTracker.Clear();
                var loaded = ctx.Tenants.Find(tenant.Id);
                Assert.NotNull(loaded);
                Assert.Equal("Test Dealer", loaded.Name);
            }
        }

        [Fact]
        public void DbContext_ApiKeyIndex_IsUnique()
        {
            var (ctx, conn) = CreateContext();
            using (conn)
            using (ctx)
            {
                ctx.Tenants.Add(new TenantEntity { Name = "A", ApiKey = "duplicate-key" });
                ctx.Tenants.Add(new TenantEntity { Name = "B", ApiKey = "duplicate-key" });
                Assert.Throws<Microsoft.EntityFrameworkCore.DbUpdateException>(() => ctx.SaveChanges());
            }
        }
    }
}
