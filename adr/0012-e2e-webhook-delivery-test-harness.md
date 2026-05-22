# ADR 0012 — E2E Webhook Delivery Test Harness

## Context

After Phase 8, the webhook delivery path was covered by two separate test layers:

- `QuoteCompletionWebhookDispatcherTests` — tests the dispatcher in isolation using a `RecordingHandler` (fake HTTP transport). Proves payload shape but does not exercise the controller.
- `QuoteSessionsControllerTests` — tests the controller using a `FakeQuoteCompletionWebhookDispatcher`. Proves the controller calls the dispatcher and persists a delivery attempt, but does not exercise the real dispatcher or the actual HTTP POST.

No single test covered the full loop: controller → real dispatcher → real HTTP call → stub receiver → delivery attempt persisted. That gap means a regression in how the dispatcher builds or delivers the payload could go undetected as long as both unit tests individually pass.

## Decision

Add a dedicated E2E harness in `WindowConfigurator.Tests/Webhooks/WebhookE2EHarnessTests.cs` that wires the real `QuoteCompletionWebhookDispatcher` (not a fake) into the real `QuoteSessionsController`, backed by a `StubWebhookReceiver` — a custom `HttpMessageHandler` that intercepts the outbound HTTP call, captures the request body, and returns a configured status code.

The harness asserts:

- The stub received exactly one HTTP call
- The captured payload deserializes to a valid `quote.completed` shape with correct `Session`, `OrderGroups`, and `Items` fields
- A `WebhookDeliveryAttemptEntity` was persisted with the correct status, status code, session ID, and tenant ID
- On a 503 response, the attempt is marked `Failed` and `NextRetryAtUtc` is set

## Why stub HTTP transport rather than a real test server

A `TestServer` from `Microsoft.AspNetCore.TestServer` would add a full ASP.NET Core host to the test — extra setup cost, integration with routing and middleware, and a new dependency — for a receiver that does nothing except record the incoming request body. An `HttpMessageHandler` subclass achieves the same interception point with no additional infrastructure. The goal is to verify the integration seam between the dispatcher and the controller, not to test a real HTTP server.

## Why local DTOs for payload assertions

The test deserializes the captured body into local `TestPayload` / `TestSession` / `TestOrderGroup` / `TestItem` DTOs rather than reusing `QuoteCompletedPayload` directly. This decouples the assertion from the internal type. If the payload class is refactored (field rename, new property), the test will catch it via a deserialization mismatch rather than silently passing because the internal type changed alongside the assertion.

## Alternatives Considered

- **Deserializing to `QuoteCompletedPayload` directly** — simpler, but creates tight coupling between the test and the internal type. Chosen against.
- **`Microsoft.AspNetCore.TestServer` stub receiver** — more realistic but adds unnecessary host infrastructure for what is an in-process assertion. Chosen against.
- **Expanding `QuoteCompletionWebhookDispatcherTests`** — possible, but the test goal is the controller-to-dispatcher integration, not dispatcher-only. Extending the dispatcher tests would blur that boundary.

## Consequences

- Any regression in how `QuoteCompletionWebhookDispatcher.BuildPayload` maps entity fields to the payload shape will fail the harness.
- Any regression in how `QuoteSessionsController.Complete` wires the dispatcher call or persists the delivery attempt will fail the harness.
- The harness does not cover network-level concerns (TLS, DNS, timeouts beyond what `HttpMessageHandler` provides). Those remain the responsibility of production ops.
