# ADR 0003 — Session-Centric Domain Model

**Date:** 2026-05-13
**Status:** Accepted

---

## Context

The original codebase modeled the domain as **orders** (`OrderEntity`, `OrderItemEntity`), reflecting its origin in a NALP commerce integration. When the platform was pivoted to a CRM-agnostic model, this order-centric framing was carried forward without being re-evaluated.

After reviewing the architecture, this framing was found to be wrong in several ways:

- "Order" implies a purchase has been made and inventory/fulfillment logic applies. This platform handles *pre-order* configuration, not post-order fulfillment.
- An order implies a definite customer, a definite price, and a commitment. At the time this platform operates, none of those are necessarily true.
- `OrderItemEntity` was essentially empty (just a Guid) because nobody had worked out what an order item actually meant in this context.
- The CRM owns the downstream sales lifecycle, including what becomes an actual order. This platform only owns the configuration state that leads up to that.

---

## Decision

**Replace the order-centric model with a session-centric, three-level domain model:**

```
Tenant
  └── QuoteSession
        └── ConfiguredWindowItem
```

### Tenant
The window dealer or manufacturer who has licensed the platform. Owns API credentials, webhook callback URL, product line access, and branding configuration.

### QuoteSession
A single customer interaction — one job, one quote, one visit. Contains one or more configured windows. Has a lifecycle: Draft → Completed → Submitted (or Expired). Knows whether it was CRM-initiated or website-initiated. This is what the webhook fires for on completion.

### ConfiguredWindowItem
A single configured window within a session. Has its own Draft → Completed lifecycle. Holds the full configuration snapshot as a JSON blob, plus promoted scalar fields for querying (dimensions, product line, authoritative price, section count).

---

## Alternatives Considered

**Keep the order-centric model and flesh out OrderItemEntity (rejected)**
Rejected because the conceptual mismatch was not just naming — it would have leaked "order" semantics (fulfillment, inventory, payment) into a system that deliberately has none of those. Renaming is cheaper than fighting the wrong abstraction forever.

**Flat model — no session container, just items (rejected)**
Skip the `QuoteSession` container and manage items directly. Rejected because the website prospect flow requires a container for multiple configured windows and an aggregate total. Without a session container there is no natural place for the magic link, expiry, status lifecycle, or multi-item aggregation.

**Full order management (rejected)**
Build a real OMS with inventory, fulfillment, payment. Rejected as out of scope. The CRM already owns that. This platform's boundary ends at handing off a completed configuration payload.

---

## Consequences

- `OrderEntity` and `OrderItemEntity` are superseded by `QuoteSessionEntity` and `ConfiguredWindowItemEntity`. The old in-memory repository and its hardcoded Salesforce user IDs are also superseded.
- The platform's stated scope is now precise: **WindowConfigurator owns pre-order configuration aggregation. The CRM owns post-submission sales lifecycle.**
- The session model supports both single-window (CRM-initiated) and multi-window (website prospect) use cases without structural changes.
- "Quote" language should be used throughout the UI and API rather than "order" language, to correctly set expectations with both contractors and prospects.
