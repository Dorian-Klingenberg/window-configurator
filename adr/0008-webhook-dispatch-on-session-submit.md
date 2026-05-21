# ADR 0008: Webhook Dispatch On Session Submit

**Date:** 2026-05-21  
**Status:** Accepted

## Context

Phase 7 requires operational webhook handoff once a quote session is submitted. Phase 6 introduced the versioned session API, including `POST /api/v1/quote-sessions/{id}/complete`, but did not actually dispatch webhooks.

We needed a first implementation slice that:

- dispatches the `QuoteCompletedPayload` at submit time,
- keeps submission semantics stable,
- is extensible for retry and delivery tracking.

## Decision

1. Introduce `IQuoteCompletionWebhookDispatcher` and a concrete `QuoteCompletionWebhookDispatcher` using `HttpClient`.
2. Trigger webhook dispatch from `POST /api/v1/quote-sessions/{id}/complete` after session status moves to `Submitted`.
3. Return webhook outcome metadata in completion response (`WebhookDispatchStatus`, status code, error) while still returning a successful submit response when dispatch fails.
4. Keep retry policy and durable delivery logging out of this first slice; implement them in subsequent Phase 7 increments.

## Alternatives Considered

1. Fail submit when webhook delivery fails.
   - Rejected for first slice because submission and downstream delivery reliability are related but distinct concerns. We want explicit delivery outcome without forcing operators to resubmit sessions as a retry mechanism.

2. Add full retry + persistence in one step.
   - Rejected to keep the first webhook slice narrow and testable.

3. Keep submit endpoint unchanged and dispatch webhooks asynchronously with no response metadata.
   - Rejected because callers need immediate visibility that dispatch was attempted and whether it succeeded.

## Consequences

1. Session submission now performs real outbound webhook delivery attempts.
2. API callers get immediate delivery attempt visibility from the completion response.
3. The architecture now has a clear seam (`IQuoteCompletionWebhookDispatcher`) for adding retry/backoff and delivery logs next.
