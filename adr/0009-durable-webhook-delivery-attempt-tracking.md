# ADR 0009: Durable Webhook Delivery Attempt Tracking

**Date:** 2026-05-21  
**Status:** Accepted

## Context

Phase 7 introduced webhook dispatch on session submit, but a transient HTTP call alone is not enough for operational reliability. We need a durable record of each delivery attempt so failures can be inspected and retried without relying on ephemeral logs.

## Decision

1. Introduce `WebhookDeliveryAttemptEntity` as a persisted record of each dispatch attempt.
2. Persist a delivery-attempt row immediately after each submit-time dispatch attempt.
3. Record first retry metadata on failures (`NextRetryAtUtc = attemptedAt + 5 minutes`) to establish retry orchestration hooks.
4. Keep retry execution out of this slice; this ADR covers persistence and metadata, not background scheduling.

## Alternatives Considered

1. Use application logs only.
   - Rejected because logs are not a durable workflow state or retry queue.

2. Implement full queue + worker + exponential retries in one pass.
   - Rejected for this slice to keep scope tight and make progress incrementally.

3. Retry inline inside submit endpoint.
   - Rejected because it can increase API latency and blur transaction boundaries.

## Consequences

1. Every dispatch attempt now has a durable traceable record.
2. Failed attempts are discoverable and carry retry scheduling metadata.
3. The system now has a clean persistence seam for the next Phase 7 slices:
   - retry worker execution
   - exponential backoff policy
   - delivery observability/reporting.
