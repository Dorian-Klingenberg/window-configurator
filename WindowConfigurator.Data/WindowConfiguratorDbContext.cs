using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data
{
    public class WindowConfiguratorDbContext : DbContext
    {
        public WindowConfiguratorDbContext(DbContextOptions<WindowConfiguratorDbContext> options)
            : base(options) { }

        public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
        public DbSet<QuoteSessionEntity> QuoteSessions => Set<QuoteSessionEntity>();
        public DbSet<ConfiguredWindowItemEntity> ConfiguredWindowItems => Set<ConfiguredWindowItemEntity>();
        public DbSet<WebhookDeliveryAttemptEntity> WebhookDeliveryAttempts => Set<WebhookDeliveryAttemptEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantEntity>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.ApiKey).IsUnique();

                // Branding stored as inline columns on the Tenants table
                entity.OwnsOne(t => t.Branding);

                // AllowedProductLineKeys stored as a comma-delimited string column
                entity.Property(t => t.AllowedProductLineKeys)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => string.IsNullOrEmpty(v)
                            ? new List<string>()
                            : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            });

            modelBuilder.Entity<QuoteSessionEntity>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.HasIndex(q => q.TenantId);

                // A session belongs to a tenant; deleting a tenant is restricted while sessions exist
                entity.HasOne<TenantEntity>()
                    .WithMany()
                    .HasForeignKey(q => q.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Deleting a session cascades to its items
                entity.HasMany(q => q.Items)
                    .WithOne()
                    .HasForeignKey(i => i.QuoteSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ConfiguredWindowItemEntity>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasIndex(i => i.QuoteSessionId);
            });

            modelBuilder.Entity<WebhookDeliveryAttemptEntity>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.QuoteSessionId);
                entity.HasIndex(x => x.TenantId);
                entity.HasIndex(x => x.AttemptedAtUtc);
            });
        }
    }
}
