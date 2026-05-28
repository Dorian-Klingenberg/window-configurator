namespace MockContractorCrm.Services;

public interface ICrmOpportunityStore
{
    Task<CrmOpportunity> CreateAsync(string customerName, string customerEmail);
    Task<IReadOnlyList<CrmOpportunity>> ListAsync();
    Task<CrmOpportunity?> GetAsync(Guid id);
    Task MarkQuoteStartedAsync(Guid id, string launchUrl, Guid sessionId);
    Task MarkQuoteSubmittedAsync(Guid id, DateTime submittedAt, int itemCount, decimal? totalPrice);
}

public sealed class InMemoryCrmOpportunityStore : ICrmOpportunityStore
{
    private readonly List<CrmOpportunity> _items = [];
    private int _sequence = 1000;

    public Task<CrmOpportunity> CreateAsync(string customerName, string customerEmail)
    {
        var item = new CrmOpportunity(
            Guid.NewGuid(),
            $"OPP-{Interlocked.Increment(ref _sequence)}",
            customerName.Trim(),
            customerEmail.Trim(),
            "Open",
            null,
            null,
            DateTime.UtcNow);

        lock (_items)
            _items.Add(item);

        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<CrmOpportunity>> ListAsync()
    {
        lock (_items)
            return Task.FromResult((IReadOnlyList<CrmOpportunity>)_items.OrderByDescending(i => i.CreatedAtUtc).ToList());
    }

    public Task<CrmOpportunity?> GetAsync(Guid id)
    {
        lock (_items)
            return Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
    }

    public Task MarkQuoteStartedAsync(Guid id, string launchUrl, Guid sessionId)
    {
        lock (_items)
        {
            var index = _items.FindIndex(i => i.Id == id);
            if (index < 0)
                return Task.CompletedTask;

            var existing = _items[index];
            _items[index] = existing with
            {
                Status = "Quote Started",
                LaunchUrl = launchUrl,
                QuoteSessionId = sessionId
            };
        }

        return Task.CompletedTask;
    }

    public Task MarkQuoteSubmittedAsync(Guid id, DateTime submittedAt, int itemCount, decimal? totalPrice)
    {
        lock (_items)
        {
            var index = _items.FindIndex(i => i.Id == id);
            if (index < 0)
                return Task.CompletedTask;

            var existing = _items[index];
            _items[index] = existing with
            {
                Status = "Quote Submitted",
                SubmittedAt = submittedAt,
                CompletedItemCount = itemCount,
                AuthoritativeTotalPrice = totalPrice
            };
        }

        return Task.CompletedTask;
    }
}
