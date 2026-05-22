using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public interface IQuoteSessionRepository
    {
        Task<QuoteSessionEntity?> GetByIdAsync(Guid id);
        Task<QuoteSessionEntity?> GetByMagicLinkTokenAsync(string token);
        Task<QuoteSessionEntity?> GetFirstAsync();
        Task<IReadOnlyList<QuoteSessionEntity>> ListByTenantAsync(Guid tenantId);
        Task AddAsync(QuoteSessionEntity session);
        Task AddItemAsync(Guid sessionId, ConfiguredWindowItemEntity item);
        Task SaveChangesAsync();
    }
}
