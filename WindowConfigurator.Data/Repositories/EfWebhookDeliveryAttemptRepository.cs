using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public class EfWebhookDeliveryAttemptRepository : IWebhookDeliveryAttemptRepository
    {
        private readonly WindowConfiguratorDbContext _context;

        public EfWebhookDeliveryAttemptRepository(WindowConfiguratorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WebhookDeliveryAttemptEntity attempt)
            => await _context.WebhookDeliveryAttempts.AddAsync(attempt);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<WebhookDeliveryStats> GetStatsAsync()
        {
            var total = await _context.WebhookDeliveryAttempts.CountAsync();
            var delivered = await _context.WebhookDeliveryAttempts.CountAsync(a => a.Status == "Delivered");
            var failed = await _context.WebhookDeliveryAttempts.CountAsync(a => a.Status == "Failed");
            return new WebhookDeliveryStats { Total = total, Delivered = delivered, Failed = failed };
        }
    }
}
