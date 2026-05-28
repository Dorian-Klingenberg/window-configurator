using MockContractorCrm.Services;

namespace WindowConfigurator.Tests.Services;

public class CrmOpportunityStoreTests
{
    [Fact]
    public async Task MarkQuoteSubmittedAsync_SetsSubmittedFieldsOnOpportunity()
    {
        var store = new InMemoryCrmOpportunityStore();
        var opportunity = await store.CreateAsync("Alice Smith", "alice@example.com");

        var launchUrl = "http://localhost:5149/session-abc";
        var sessionId = Guid.NewGuid();
        await store.MarkQuoteStartedAsync(opportunity.Id, launchUrl, sessionId);

        var submittedAt = new DateTime(2026, 5, 27, 14, 0, 0, DateTimeKind.Utc);
        await store.MarkQuoteSubmittedAsync(opportunity.Id, submittedAt, itemCount: 3, totalPrice: 4567.89m);

        var updated = await store.GetAsync(opportunity.Id);
        Assert.NotNull(updated);
        Assert.Equal("Quote Submitted", updated!.Status);
        Assert.Equal(submittedAt, updated.SubmittedAt);
        Assert.Equal(3, updated.CompletedItemCount);
        Assert.Equal(4567.89m, updated.AuthoritativeTotalPrice);
    }

    [Fact]
    public async Task MarkQuoteSubmittedAsync_WhenOpportunityNotFound_DoesNotThrow()
    {
        var store = new InMemoryCrmOpportunityStore();

        // Should not throw even for unknown id
        await store.MarkQuoteSubmittedAsync(Guid.NewGuid(), DateTime.UtcNow, 0, null);
    }

    [Fact]
    public async Task MarkQuoteSubmittedAsync_WithNullPrice_SetsNullTotalPrice()
    {
        var store = new InMemoryCrmOpportunityStore();
        var opportunity = await store.CreateAsync("Bob Jones", "bob@example.com");
        await store.MarkQuoteStartedAsync(opportunity.Id, "http://example.com/session", Guid.NewGuid());

        await store.MarkQuoteSubmittedAsync(opportunity.Id, DateTime.UtcNow, itemCount: 1, totalPrice: null);

        var updated = await store.GetAsync(opportunity.Id);
        Assert.NotNull(updated);
        Assert.Equal("Quote Submitted", updated!.Status);
        Assert.Equal(1, updated.CompletedItemCount);
        Assert.Null(updated.AuthoritativeTotalPrice);
    }
}
