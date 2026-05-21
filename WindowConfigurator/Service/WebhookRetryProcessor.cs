using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;

namespace WindowConfigurator.Web.Service
{
    public class WebhookRetryProcessor : IWebhookRetryProcessor
    {
        private readonly WindowConfiguratorDbContext _context;
        private readonly IQuoteCompletionWebhookDispatcher _dispatcher;
        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICatalogService _catalog;

        public WebhookRetryProcessor(
            WindowConfiguratorDbContext context,
            IQuoteCompletionWebhookDispatcher dispatcher,
            IQuoteSessionRepository sessionRepository,
            ITenantRepository tenantRepository,
            ICatalogService catalog)
        {
            _context = context;
            _dispatcher = dispatcher;
            _sessionRepository = sessionRepository;
            _tenantRepository = tenantRepository;
            _catalog = catalog;
        }

        public async Task<int> ProcessDueRetriesAsync(int maxAttempts = 50, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var dueAttempts = await _context.WebhookDeliveryAttempts
                .Where(a => a.Status == "Failed" && a.NextRetryAtUtc != null && a.NextRetryAtUtc <= now)
                .OrderBy(a => a.NextRetryAtUtc)
                .Take(maxAttempts)
                .ToListAsync(cancellationToken);

            foreach (var attempt in dueAttempts)
            {
                var session = await _sessionRepository.GetByIdAsync(attempt.QuoteSessionId);
                var tenant = await _tenantRepository.GetByIdAsync(attempt.TenantId);

                attempt.AttemptCount += 1;
                attempt.AttemptedAtUtc = now;

                if (session == null || tenant == null)
                {
                    attempt.Status = "Failed";
                    attempt.Error = "Retry failed: missing session or tenant.";
                    attempt.NextRetryAtUtc = now.Add(CalculateBackoff(attempt.AttemptCount));
                    continue;
                }

                var dispatch = await _dispatcher.DispatchQuoteCompletedAsync(session, tenant, _catalog, cancellationToken);
                attempt.StatusCode = dispatch.StatusCode;
                attempt.Error = dispatch.Error;

                if (dispatch.Succeeded)
                {
                    attempt.Status = "Delivered";
                    attempt.NextRetryAtUtc = null;
                }
                else
                {
                    attempt.Status = "Failed";
                    attempt.NextRetryAtUtc = now.Add(CalculateBackoff(attempt.AttemptCount));
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return dueAttempts.Count;
        }

        private static TimeSpan CalculateBackoff(int attemptCount)
        {
            var minutes = Math.Min(60, 5 * Math.Pow(2, Math.Max(0, attemptCount - 1)));
            return TimeSpan.FromMinutes(minutes);
        }
    }
}
