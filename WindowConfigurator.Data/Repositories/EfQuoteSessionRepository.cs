using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public class EfQuoteSessionRepository : IQuoteSessionRepository
    {
        private readonly WindowConfiguratorDbContext _context;

        public EfQuoteSessionRepository(WindowConfiguratorDbContext context)
        {
            _context = context;
        }

        public async Task<QuoteSessionEntity?> GetByIdAsync(Guid id)
            => await _context.QuoteSessions
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<QuoteSessionEntity?> GetByMagicLinkTokenAsync(string token)
            => await _context.QuoteSessions
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.MagicLinkToken == token);

        public async Task<QuoteSessionEntity?> GetFirstAsync()
            => await _context.QuoteSessions
                .Include(s => s.Items)
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync();

        public async Task<IReadOnlyList<QuoteSessionEntity>> ListByTenantAsync(Guid tenantId)
            => await _context.QuoteSessions
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Items)
                .ToListAsync();

        public async Task AddAsync(QuoteSessionEntity session)
            => await _context.QuoteSessions.AddAsync(session);

        public async Task AddItemAsync(Guid sessionId, ConfiguredWindowItemEntity item)
        {
            item.QuoteSessionId = sessionId;
            await _context.ConfiguredWindowItems.AddAsync(item);
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
