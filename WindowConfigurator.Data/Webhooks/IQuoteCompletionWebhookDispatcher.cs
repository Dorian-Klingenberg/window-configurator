using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Webhooks
{
    public interface IQuoteCompletionWebhookDispatcher
    {
        Task<QuoteCompletionWebhookDispatchResult> DispatchQuoteCompletedAsync(
            QuoteSessionEntity session,
            TenantEntity tenant,
            ICatalogService catalog,
            CancellationToken cancellationToken = default);
    }
}
