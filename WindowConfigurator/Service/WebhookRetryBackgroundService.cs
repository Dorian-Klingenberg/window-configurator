using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WindowConfigurator.Data.Webhooks;

namespace WindowConfigurator.Web.Service
{
    /// <summary>
    /// Polls for webhook delivery attempts that are due for retry and processes them
    /// on a configurable interval. Replaces the manual POST /api/v1/webhook-deliveries/retry-due
    /// trigger for production use.
    /// Configure interval via Webhooks:RetryIntervalSeconds (default: 300).
    /// </summary>
    public class WebhookRetryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollInterval;

        public WebhookRetryBackgroundService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _pollInterval = TimeSpan.FromSeconds(
                configuration.GetValue("Webhooks:RetryIntervalSeconds", 300));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_pollInterval, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IWebhookRetryProcessor>();
                await processor.ProcessDueRetriesAsync(cancellationToken: stoppingToken);
            }
        }
    }
}
