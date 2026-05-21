# ADR 0007: Minimal Versioned Session API Surface

**Date:** 2026-05-21  
**Status:** Accepted

## Context

Before Phase 6, the application behavior was mostly exposed through MVC controller routes intended for the configurator UI. That made CRM and website integration planning difficult because there was no stable versioned API contract for session lifecycle operations.

Phase 5 already established server-authoritative catalog resolution, validation, and pricing. Phase 6 needed to expose those capabilities through a minimal but durable API boundary without prematurely typing the full nested configurator payload.

## Decision

Introduce and standardize a minimal versioned session API at `/api/v1/quote-sessions` with DTO contracts for session and item lifecycle operations:

- `POST /api/v1/quote-sessions`
- `GET /api/v1/quote-sessions/{id}`
- `PUT /api/v1/quote-sessions/{id}`
- `POST /api/v1/quote-sessions/{id}/items`
- `PUT /api/v1/quote-sessions/{id}/items/{itemId}`
- `POST /api/v1/quote-sessions/{id}/complete`

Adopt shared API error contracts:

- `ApiErrorResponse`
- `ApiValidationError`

Adopt explicit completion DTOs:

- `CompleteQuoteSessionRequest`
- `CompleteQuoteSessionResponse`

Keep full configuration snapshots as JSON blobs for now, rather than forcing a full typed DTO graph for every nested configurator field in this phase.

## Alternatives Considered

1. Keep using only MVC-style endpoints until authentication/webhooks are built.
   - Rejected because integration consumers need stable contracts now, and waiting blocks downstream work.

2. Fully type the complete configurator payload immediately.
   - Rejected because it adds heavy churn before integration/auth concerns stabilize and is not required to make session APIs usable.

3. Expose API endpoints but continue mixed plain-string and anonymous error responses.
   - Rejected because inconsistent error shapes create fragile client integrations.

## Consequences

1. Integration consumers now have a stable versioned session/item lifecycle API surface.
2. Error handling is more predictable due to shared envelope contracts.
3. Completion responses are now explicit DTOs, reducing ambiguity for clients.
4. The MVC completion bridge still exists for the existing configurator flow, but Phase 6 closes with a clean API baseline for Phase 7 webhook and tenant-integration work.
