namespace WindowConfigurator.Data.Entities
{
    public class WebhookDeliveryAttemptEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuoteSessionId { get; set; }
        public Guid TenantId { get; set; }
        public string EventType { get; set; } = "quote.completed";
        public string Status { get; set; } = "Pending";
        public int AttemptCount { get; set; } = 1;
        public int? StatusCode { get; set; }
        public string? Error { get; set; }
        public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? NextRetryAtUtc { get; set; }
    }
}
