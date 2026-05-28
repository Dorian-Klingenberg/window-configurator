# ADR 0018 — CRM Completion Status via On-Demand Polling and Session API Extension

**Date:** 2026-05-27
**Status:** Accepted

---

## Context

MockContractorCrm (ADR 0015) maintains an in-memory list of opportunities. Each opportunity can be in one of three states: created (no session), quote started (session exists), or quote submitted (session completed). The CRM operator needs to know when a session has completed so the opportunity can be updated to "Quote Submitted" with authoritative price and item count.

This is distinct from the website-initiated flow (ADR 0017), where the completion signal is delivered browser-to-browser via postMessage. The CRM flow is server-to-server: the CRM backend needs to learn from the Configurator API whether a session has completed.

The webhook mechanism (ADR 0001, ADR 0008) exists for this purpose — but it fires to the tenant's registered callback URL, which in a real deployment is the CRM's inbound webhook endpoint. MockContractorCrm does not register itself as a webhook receiver for its own demo tenant; it has no inbound webhook handler. This is by design: the mock does not need to model inbound webhook receipt to demonstrate the completion status story.

The GET session endpoint (`GET /api/v1/quote-sessions/{id}`) existed before Phase 10 but returned only session-level fields and item configuration snapshots. It did not expose `AuthoritativePrice` or `CompletedAt` at the item level, even though `ConfiguredWindowItemEntity` already carried those fields.

---

## Decision

**Two changes made together:**

### 1. Extend `GET /api/v1/quote-sessions/{id}` to expose completion data per item

Add `AuthoritativePrice` (decimal?) and `CompletedAt` (DateTime?) to `QuoteSessionItemResponse`. Map them from `ConfiguredWindowItemEntity` in `QuoteSessionsController.ToItemResponse()`.

This makes the existing GET session endpoint sufficient as a completion status probe: a caller can fetch the session, check the session-level `Status`, and inspect per-item `AuthoritativePrice` and `CompletedAt` without a dedicated status endpoint.

### 2. Add `POST /api/opportunities/{id}/refresh-status` to MockContractorCrm

This endpoint:
1. Resolves the opportunity by ID from the in-memory store
2. Calls `CrmQuoteSessionClient.GetSessionStatusAsync(sessionId)` — which calls `GET /api/v1/quote-sessions/{id}` with the API key
3. If the session status is "Completed" or "Submitted", calls `MarkQuoteSubmittedAsync` to update the opportunity record with item count and total authoritative price
4. Returns the current status metadata as JSON

The MockContractorCrm UI shows a "Refresh Status" button for in-progress opportunities. The operator clicks it when they want to check. The result updates the opportunity row in place.

---

## Alternatives Considered

**Register MockContractorCrm as a webhook receiver (rejected)**
Implement an inbound webhook handler in MockContractorCrm that receives the `quote.completed` event and updates the opportunity automatically. Rejected because: (1) the mock CRM does not represent a real registered tenant webhook endpoint; (2) adding inbound webhook receipt would require the mock to expose a public URL and be registered as the tenant's callback, which conflicts with the demo setup; (3) on-demand polling is simpler and makes the demo step more explicit — the operator clicks "Refresh Status" and sees the result immediately.

**Dedicated `/api/v1/quote-sessions/{id}/status` endpoint (rejected)**
Add a new lightweight status-only endpoint rather than extending the existing GET session response. Rejected because the item-level `AuthoritativePrice` and `CompletedAt` fields are genuinely useful data that belongs on the item response regardless of the polling use case. Extending the existing endpoint costs nothing extra and avoids a proliferation of narrow status-only endpoints. Any caller that was already using `GET /api/v1/quote-sessions/{id}` automatically benefits.

**Server-sent events or WebSockets for real-time CRM status updates (rejected)**
Push completion events from the Configurator to the CRM as they happen. Rejected for demo stage because it adds infrastructure complexity (persistent connections, connection management) with no benefit over on-demand polling in a demo context where the operator is watching the screen and can click a button.

**Automatic background polling in MockContractorCrm (rejected)**
Run a timer in MockContractorCrm that polls all in-progress sessions periodically without requiring the operator to click. Rejected because it obscures the integration mechanism and makes the demo less legible. The explicit "Refresh Status" button makes the polling relationship between CRM and Configurator visible, which is the point of the demo.

---

## Consequences

- `GET /api/v1/quote-sessions/{id}` now returns `AuthoritativePrice` and `CompletedAt` per item. Any existing caller that was ignoring unknown fields will continue to work. Any caller that was relying on those fields being absent will see new data — this is a safe additive change.
- The CRM completion check is entirely mediated by `CrmQuoteSessionClient`, which handles the `X-Api-Key` header and the null-on-404 behavior. The endpoint handler in MockContractorCrm contains no direct HTTP plumbing.
- Total authoritative price is computed client-side in `GetSessionStatusAsync` by summing item `AuthoritativePrice` values. This is acceptable because the sum is a derived read-only value, not stored as authoritative on the session entity itself.
- The `MarkQuoteSubmittedAsync` path is idempotent: calling "Refresh Status" on an already-submitted opportunity is a no-op (the fields are overwritten with the same values).
- This pattern — extend the existing API + add a host-side polling trigger — is the correct approach for a demo CRM. A production CRM would instead consume the webhook (ADR 0001) and not need a polling endpoint at all.
