using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public interface IWebhookDeliveryAttemptRepository
    {
        Task AddAsync(WebhookDeliveryAttemptEntity attempt);
        Task SaveChangesAsync();
    }
}
