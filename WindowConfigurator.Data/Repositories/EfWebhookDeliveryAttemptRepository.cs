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
    }
}
