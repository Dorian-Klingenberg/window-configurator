namespace WindowConfigurator.Data.Entities
{
    public class QuoteSessionEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        public QuoteSessionStatus Status { get; set; } = QuoteSessionStatus.Draft;

        /// <summary>
        /// The product line pre-selected for this session, typically provided by the CRM at session
        /// creation time. Acts as the default for new items. Not enforced when the tenant policy
        /// allows mixed product lines — in that case each ConfiguredWindowItem carries its own.
        /// Null for website-initiated sessions where the prospect chooses freely.
        /// </summary>
        public string? DefaultProductLineKey { get; set; }

        /// <summary>Customer email address. Required for website-initiated (prospect) sessions.</summary>
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// The CRM's opportunity, record, or line item ID. Populated for CRM-initiated sessions.
        /// Passed back in the completion webhook payload so the CRM can correlate the result.
        /// </summary>
        public string? ExternalReferenceId { get; set; }

        /// <summary>
        /// Passwordless token sent to the prospect's email for website-initiated sessions.
        /// Null for CRM-initiated sessions.
        /// </summary>
        public string? MagicLinkToken { get; set; }

        public DateTime? MagicLinkExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public List<ConfiguredWindowItemEntity> Items { get; set; } = new();
    }
}
