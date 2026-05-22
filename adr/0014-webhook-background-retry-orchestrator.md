# ADR 0014 — Webhook Background Retry Orchestrator

## Context

After Phase 7, failed webhook delivery attempts were retried via a manual trigger endpoint (`POST /api/v1/webhook-deliveries/retry-due`). This required an external scheduler (cron, CRM, etc.) to call the endpoint. In production, relying on a manual trigger means retries only happen when something actively calls the endpoint — a fragile operational dependency.

## Decision

Add `WebhookRetryBackgroundService : BackgroundService` that:

1. Runs as a hosted service registered with the ASP.NET Core DI container via `AddHostedService<WebhookRetryBackgroundService>()`
2. Loops indefinitely, sleeping for a configurable interval (`Webhooks:RetryIntervalSeconds`, default 300 seconds) between iterations
3. On each iteration, creates a new DI scope and resolves `IWebhookRetryProcessor` (which is scoped, requiring a scope per call)
4. Calls `ProcessDueRetriesAsync()` and discards the count (operational visibility is provided by the stats endpoint)

The manual `POST /retry-due` endpoint is retained for operator-triggered immediate retries and for use in integration tests.

Also add `GET /api/v1/webhook-deliveries/stats` returning delivered/failed/total counts for operational monitoring.

## Why `BackgroundService` over a dedicated queue

A message queue (Azure Service Bus, RabbitMQ, etc.) would provide better durability, visibility, and scaling. However, the current system already has durable delivery attempt tracking in the database. A `BackgroundService` that polls that table is simpler, has no new infrastructure dependencies, and is correct for the current scale. The queue can be introduced later if volume demands it.

## Why `IServiceScopeFactory` rather than direct injection

`IWebhookRetryProcessor` is registered as scoped (it depends on `WindowConfiguratorDbContext` which is also scoped). `BackgroundService` instances are singletons. A singleton cannot directly depend on a scoped service — doing so would capture the scoped service for the lifetime of the singleton, causing stale DbContext issues. Using `IServiceScopeFactory` to create a fresh scope per iteration is the standard pattern for this scenario.

## Why the poll interval defaults to 300 seconds

Five minutes is a reasonable initial retry window that balances responsiveness against load. The exponential backoff in `WebhookRetryProcessor` already ensures that repeatedly failing deliveries wait progressively longer between attempts. The background service interval is a floor, not a guarantee.

## Consequences

- Webhook retries now run automatically without any external scheduler dependency.
- The poll interval is configurable without code changes via `appsettings.json`.
- Each iteration creates and disposes a DI scope, keeping DbContext lifetimes correct.
- The manual retry endpoint is still useful for operator-initiated immediate retries.
- The stats endpoint provides a lightweight operational view without a full dashboard.
