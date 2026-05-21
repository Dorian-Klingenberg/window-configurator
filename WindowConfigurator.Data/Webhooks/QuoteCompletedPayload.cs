namespace WindowConfigurator.Data.Webhooks
{
    /// <summary>
    /// Root payload POSTed to a tenant's registered callback URL when a quote session completes.
    /// The receiving system (CRM, middleware, etc.) should treat this as the authoritative
    /// description of the configured windows. The platform guarantees this shape is stable.
    /// </summary>
    public class QuoteCompletedPayload
    {
        public string EventType => "quote.completed";

        public DateTime OccurredAt { get; set; }

        public QuoteCompletedSessionSummary Session { get; set; } = new();

        /// <summary>
        /// Items grouped by product line. Most quotes will have a single group.
        /// Quotes with mixed product lines produce multiple groups — one per product line —
        /// because manufacturer purchase orders cannot span product lines.
        /// </summary>
        public List<QuoteCompletedOrderGroup> OrderGroups { get; set; } = new();
    }

    public class QuoteCompletedSessionSummary
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        /// <summary>
        /// The CRM's opportunity or record ID, passed in at session creation and echoed back
        /// here so the CRM can correlate this payload to its own record without storing our ID.
        /// Null for website-initiated sessions unless the CRM created the session.
        /// </summary>
        public string? ExternalReferenceId { get; set; }

        /// <summary>Customer email. Always present for website-initiated sessions.</summary>
        public string? CustomerEmail { get; set; }

        public DateTime CompletedAt { get; set; }

        public int ItemCount { get; set; }

        /// <summary>
        /// Sum of all item authoritative prices. Null only when aggregate pricing has not
        /// been computed yet.
        /// </summary>
        public decimal? TotalAuthoritativePrice { get; set; }
    }

    /// <summary>
    /// A group of configured windows sharing the same product line.
    /// Corresponds to one manufacturer purchase order when the quote converts to an order.
    /// </summary>
    public class QuoteCompletedOrderGroup
    {
        public string ProductLineKey { get; set; } = string.Empty;
        public string ProductLineName { get; set; } = string.Empty;
        public string ManufacturerName { get; set; } = string.Empty;

        public List<QuoteCompletedItem> Items { get; set; } = new();

        /// <summary>Sum of authoritative prices for items in this group.</summary>
        public decimal? GroupAuthoritativePrice { get; set; }
    }

    public class QuoteCompletedItem
    {
        public Guid Id { get; set; }

        public int LineItemNumber { get; set; }

        /// <summary>Where in the building this window is installed. E.g. "Master Bedroom Left".</summary>
        public string Location { get; set; } = string.Empty;

        public bool MeetsEgress { get; set; }

        public QuoteCompletedMeasurements Measurements { get; set; } = new();

        public int SectionCount { get; set; }

        /// <summary>
        /// Server-computed price for this item. Null until completion pricing has run.
        /// </summary>
        public decimal? AuthoritativePrice { get; set; }

        /// <summary>
        /// The complete configurator state snapshot for this window. Contains all selections
        /// (style, grille, SDL, colors, pane configuration, etc.) as the user left them.
        /// The CRM may store or display this, but should not need to parse it for core workflow.
        /// </summary>
        public object? Configuration { get; set; }
    }

    public class QuoteCompletedMeasurements
    {
        public QuoteCompletedDimension Frame { get; set; } = new();

        /// <summary>
        /// Null until the contractor performs a remeasure visit and updates the item.
        /// A null rough opening signals this window is not yet ready to order.
        /// </summary>
        public QuoteCompletedDimension? RoughOpening { get; set; }

        /// <summary>
        /// Outside measurement — must fit within the existing brick or vinyl surround.
        /// Derived from frame size and brickmould configuration.
        /// </summary>
        public QuoteCompletedDimension Outside { get; set; } = new();
    }

    public class QuoteCompletedDimension
    {
        /// <summary>Width in decimal inches.</summary>
        public decimal? WidthInches { get; set; }

        /// <summary>Height in decimal inches.</summary>
        public decimal? HeightInches { get; set; }

        /// <summary>Width as a fractional string, e.g. "70 1/4". Provided for display.</summary>
        public string? WidthDisplay { get; set; }

        /// <summary>Height as a fractional string, e.g. "45 1/16". Provided for display.</summary>
        public string? HeightDisplay { get; set; }
    }
}
