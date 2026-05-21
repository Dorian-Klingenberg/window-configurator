using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public interface ITenantRepository
    {
        Task<TenantEntity?> GetByIdAsync(Guid id);
        Task<TenantEntity?> GetByApiKeyAsync(string apiKey);
        Task AddAsync(TenantEntity tenant);
        Task SaveChangesAsync();
    }
}
