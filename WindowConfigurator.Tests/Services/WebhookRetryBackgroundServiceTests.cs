using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Webhooks;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Tests.Services
{
    public class WebhookRetryBackgroundServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_OnEachInterval_CallsRetryProcessor()
        {
            var fakeProcessor = new CallCountingRetryProcessor();
            var services = new ServiceCollection();
            services.AddSingleton<IWebhookRetryProcessor>(fakeProcessor);
            var provider = services.BuildServiceProvider();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Webhooks:RetryIntervalSeconds"] = "0"
                })
                .Build();

            var service = new WebhookRetryBackgroundService(scopeFactory, config);
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

            await service.StartAsync(cts.Token);
            try { await service.ExecuteTask!; } catch (OperationCanceledException) { }

            Assert.True(fakeProcessor.CallCount >= 1, $"Expected at least 1 call, got {fakeProcessor.CallCount}");
        }

        private class CallCountingRetryProcessor : IWebhookRetryProcessor
        {
            public int CallCount { get; private set; }

            public Task<int> ProcessDueRetriesAsync(int maxAttempts = 50, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult(0);
            }
        }
    }
}
