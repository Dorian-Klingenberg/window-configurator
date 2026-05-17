# ADR 0004 — Product Line as Session-Level Input, Not Per-Window User Choice

**Date:** 2026-05-13
**Status:** Accepted

---

## Context

The configurator UI originally allowed the user to select a product line freely per window, treating it as a casual in-session choice equivalent to picking a frame color. This was a carryover from the original commerce integration.

When the platform was reviewed for real-world contractor workflows, this model was identified as misaligned with how window jobs actually work.

In practice:
- A contractor or dealer selects a product line at the **project or quote level**, not per window. A job is typically all Apex, or all EnergySaver, based on the customer's budget and the dealer's recommendation.
- The **CRM context** usually drives product line selection — the CRM creates the session knowing what product line the opportunity involves.
- Mixing product lines within a quote is legitimate for *pricing comparison*, but mixing product lines within a *purchase order* is not possible — manufacturers produce per product line, and a job with mixed lines requires separate orders.

---

## Decision

**Product line is session-level input, not arbitrary per-window user-owned state.**

Specifically:

- `QuoteSession` carries a `DefaultProductLineKey` — set at session creation, typically by the CRM for CRM-initiated sessions, or chosen by the prospect for website-initiated sessions.
- Each `ConfiguredWindowItem` carries its own `ProductLineKey` as the authoritative value, resolved from the session default at item creation time.
- The `Tenant` entity has a `MixedProductLinesAllowed` policy flag. When `false` (the default), all items in a session must share the same product line. When `true`, items may differ.
- Regardless of the tenant policy, **purchase orders are always per product line**. A completed quote with mixed product lines will produce multiple order payloads in the webhook dispatch, each scoped to a single product line.

---

## Alternatives Considered

**Fully free per-window product line selection (rejected)**
Allow users to pick any product line for any window at any time. Rejected because it does not reflect real contractor workflow, produces quotes that cannot be directly converted to orders, and gives the configurator false affordances.

**Single product line per session, strictly enforced (rejected)**
One product line for the whole session, no exceptions. Rejected because legitimate quoting scenarios exist where a contractor is comparing two product lines for the same job (e.g., pricing both Apex and EnergySaver for a customer deciding on budget). The tenant policy flag handles this without making mixing the default.

**No product line concept at the session level (rejected)**
Each item selects its own line independently with no session-level default. Rejected because it provides no useful starting point for the CRM-initiated flow where the product line is already known, and makes single-product-line jobs (the majority) require redundant per-item selection.

---

## Consequences

- The CRM-initiated session creation endpoint accepts an optional `defaultProductLineKey` parameter. If the CRM provides it, new items default to that product line.
- For website-initiated sessions, the prospect chooses a product line at session start (or the tenant has a single product line configured, removing the choice entirely).
- When `MixedProductLinesAllowed = true`, the `DefaultProductLineKey` on the session acts as a hint only. The user can override it per item.
- At quote completion, if the session contains items from multiple product lines, the webhook dispatch must group items by product line and produce a separate payload per line, or include grouping metadata in a single payload, so the receiving CRM can create separate orders appropriately.
- The catalog resolver (Phase 3) will use the item's `ProductLineKey` — not any client-provided value — to resolve templates, assets, and pricing data server-side.
