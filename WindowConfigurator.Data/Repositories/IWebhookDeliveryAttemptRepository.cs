using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public interface IWebhookDeliveryAttemptRepository
    {
        Task AddAsync(WebhookDeliveryAttemptEntity attempt);
        Task SaveChangesAsync();
        Task<WebhookDeliveryStats> GetStatsAsync();
    }

    public class WebhookDeliveryStats
    {
        public int Delivered { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
    }
}
