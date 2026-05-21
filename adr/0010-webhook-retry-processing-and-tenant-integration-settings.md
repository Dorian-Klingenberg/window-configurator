# ADR 0010: Webhook Retry Processing And Tenant Integration Settings

**Date:** 2026-05-21  
**Status:** Accepted

## Context

After introducing submit-time webhook dispatch and durable attempt tracking, the platform still needed two operational capabilities to complete Phase 7:

1. A concrete retry execution path for failed webhook attempts.
2. A minimal API surface for tenant webhook callback configuration.

Without those, delivery failures could be recorded but not systematically advanced, and tenant callback changes required direct data edits.

## Decision

1. Add retry execution service:
   - `IWebhookRetryProcessor`
   - `WebhookRetryProcessor`
2. Retry processor behavior:
   - scans due failed attempts (`Status == Failed` and `NextRetryAtUtc <= now`)
   - re-dispatches using authoritative session + tenant data
   - updates attempt status/metadata in place
   - applies exponential retry delay progression with a capped backoff window
3. Expose manual orchestration endpoint:
   - `POST /api/v1/webhook-deliveries/retry-due`
4. Add tenant integration settings endpoints:
   - `GET /api/v1/tenants/{id}/integration`
   - `PUT /api/v1/tenants/{id}/integration`
   - validates callback URL as absolute URI

## Alternatives Considered

1. Implement retries only as a background worker with no manual trigger.
   - Rejected for this step because a manual trigger API provides deterministic operational control during rollout and testing.

2. Keep tenant callback URL as data-only (no API).
   - Rejected because callback configuration is part of integration workflow and should be manageable via the platform contract.

3. Create a new delivery-attempt row per retry instead of updating attempt metadata in place.
   - Deferred. Current design keeps implementation simple while preserving essential status, count, and scheduling data.

## Consequences

1. Failed webhook attempts are now both durable and executable through retry processing.
2. Tenant webhook callback configuration is now available via versioned API.
3. Phase 7 can be considered complete with dispatch, tracking, and retry behavior in place.
4. Phase 8/operational hardening can focus on background automation, observability depth, and tenant security boundaries.
