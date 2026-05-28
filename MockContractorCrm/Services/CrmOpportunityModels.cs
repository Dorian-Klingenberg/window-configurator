namespace MockContractorCrm.Services;

public sealed record CrmOpportunity(
    Guid Id,
    string OpportunityNumber,
    string CustomerName,
    string CustomerEmail,
    string Status,
    Guid? QuoteSessionId,
    string? LaunchUrl,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAt = null,
    int? CompletedItemCount = null,
    decimal? AuthoritativeTotalPrice = null);
