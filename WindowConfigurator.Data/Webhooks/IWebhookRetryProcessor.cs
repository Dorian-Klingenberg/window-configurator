namespace WindowConfigurator.Data.Webhooks
{
    public interface IWebhookRetryProcessor
    {
        Task<int> ProcessDueRetriesAsync(int maxAttempts = 50, CancellationToken cancellationToken = default);
    }
}
