using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public class EfTenantRepository : ITenantRepository
    {
        private readonly WindowConfiguratorDbContext _context;

        public EfTenantRepository(WindowConfiguratorDbContext context)
        {
            _context = context;
        }

        public async Task<TenantEntity?> GetByIdAsync(Guid id)
            => await _context.Tenants.FindAsync(id);

        public async Task<TenantEntity?> GetByApiKeyAsync(string apiKey)
            => await _context.Tenants.FirstOrDefaultAsync(t => t.ApiKey == apiKey);

        public async Task AddAsync(TenantEntity tenant)
            => await _context.Tenants.AddAsync(tenant);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
