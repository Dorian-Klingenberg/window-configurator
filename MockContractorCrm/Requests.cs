public sealed class CreateOpportunityRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

public sealed class StartQuoteRequest
{
    public string? ProductLineKey { get; set; }
}
