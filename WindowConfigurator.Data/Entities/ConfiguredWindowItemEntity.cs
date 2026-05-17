namespace WindowConfigurator.Data.Entities
{
    public class ConfiguredWindowItemEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid QuoteSessionId { get; set; }

        public ConfiguredWindowItemStatus Status { get; set; } = ConfiguredWindowItemStatus.Draft;

        // ── Identifying fields ──────────────────────────────────────────────────

        /// <summary>Display label for where this window is installed. E.g. "Master Bedroom Left".</summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>Position of this item within the quote, for display and ordering.</summary>
        public int LineItemNumber { get; set; }

        public bool MeetsEgress { get; set; } = false;

        // ── Product line ────────────────────────────────────────────────────────

        /// <summary>
        /// Resolved from the session's DefaultProductLineKey at item creation time and stored here
        /// as the authoritative value. Survives independent of the session-level default.
        /// </summary>
        public string ProductLineKey { get; set; } = string.Empty;

        // ── Promoted queryable dimensions ───────────────────────────────────────
        //
        // Three measurements matter for window ordering:
        //
        //   Frame          — outside edge to outside edge of the inner frame (jamb extension
        //                    included). The window unit size. This is what gets ordered.
        //
        //   Rough opening  — the actual structural opening, measured by the contractor during
        //                    remeasure (after casing is pulled) before the order is placed.
        //                    Approximated as frame + ½" on all sides at quote time, but the
        //                    real value is measured independently and may differ. Null until
        //                    the contractor performs the remeasure and updates the item.
        //
        //   Outside        — the dimension that has to fit within the existing brick or vinyl
        //                    surround. Variable: depends on whether the old or new window has
        //                    brickmould, and how the original installation was done. Stored
        //                    because it cannot be computed from the frame alone.

        /// <summary>Frame width, decimal inches. Outside edge to outside edge of the inner frame.</summary>
        public decimal? FrameWidthDecimal { get; set; }

        /// <summary>Frame height, decimal inches.</summary>
        public decimal? FrameHeightDecimal { get; set; }

        /// <summary>
        /// Rough opening width, decimal inches. Measured by the contractor during remeasure
        /// (casing pulled, actual opening verified) before the order is placed. Null at quote
        /// time. Approximated as frame width + 1" until the remeasure is done.
        /// </summary>
        public decimal? RoughOpeningWidthDecimal { get; set; }

        /// <summary>
        /// Rough opening height, decimal inches. Same lifecycle as RoughOpeningWidthDecimal.
        /// </summary>
        public decimal? RoughOpeningHeightDecimal { get; set; }

        /// <summary>
        /// Outside measurement width, decimal inches.The dimension that must fit within the
        /// brick or vinyl surround. Differs from frame width when brickmould is present on the
        /// old or new window, or when the original installation affects the exterior profile.
        /// Corresponds to osmWidth in the configurator JSON payload.
        /// </summary>
        public decimal? OutsideMeasureWidthDecimal { get; set; }

        /// <summary>Outside measurement height, decimal inches.</summary>
        public decimal? OutsideMeasureHeightDecimal { get; set; }

        public int SectionCount { get; set; }

        // ── Pricing ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Server-computed authoritative price. Null until the server pricing service has run.
        /// The client-submitted price is never trusted — it exists only in the JSON snapshot below.
        /// </summary>
        public decimal? AuthoritativePrice { get; set; }

        public DateTime? PricingComputedAt { get; set; }

        // ── Full configuration snapshot ─────────────────────────────────────────

        /// <summary>
        /// The complete configurator payload as a JSON blob — the full state of the window as
        /// the user left it. This is the source for server-side validation and pricing, and the
        /// content of the completion webhook payload. Scalar fields above are promoted from this.
        /// </summary>
        public string? ConfigurationJson { get; set; }

        // ── Timestamps ──────────────────────────────────────────────────────────

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}

