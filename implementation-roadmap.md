# WindowConfigurator Implementation Roadmap

This roadmap is ordered by risk reduction first, then by implementation effort. The intent is to keep the current configurator UI working while turning the application into a trustworthy service.

## Planning Assumptions

- The current configurator UI and interaction model stay in place for now.
- The CRM remains the downstream system of record for post-submission sales workflow.
- WindowConfigurator becomes the system of record for pre-submission configuration state.
- Product line is usually assigned by session or quote context, not chosen freely per window.
- Server-side authority is required for validation, pricing, persistence, and completion.

## Delivery Principles

1. Prefer changes that reduce business and integration risk before changes that improve elegance.
2. Preserve a runnable demo at every phase.
3. Do not start with a frontend rewrite.
4. Keep the API CRM-agnostic.
5. Treat browser-side pricing and validation as convenience, not authority.

## Phase 0: Stabilize The Demo Surface ✅ Complete

Risk reduced: misleading product behavior, confusing demos, false expectations.
Effort: low.

### Goals

- Make the current app honest about what it does.
- Keep a stable demo entry point.
- Remove false affordances that imply unsupported capabilities.

### Changes

1. Remove or hide Create, Details, Delete, and fake Save flows that are not backed by real server behavior.
2. Make the demo route explicit so the app opens to a known working sample path.
3. Reframe internal language from order management to configurator session or quote-building where appropriate.
4. Document the current product boundary clearly in the repo.

### Why this comes first

These changes are cheap and immediately reduce confusion for demos, planning, and architecture work.

## Phase 1: Define The Real Backend Shape ✅ Complete

Risk reduced: wrong architecture, wrong product boundary, wasted implementation effort.
Effort: low to medium.

### Goals

- Replace the fake order-centric boundary with a real session-centric model.
- Introduce the minimum domain objects needed for both CRM and website flows.

### Changes

1. Define `Tenant` as the owner of branding, catalog access, pricing policy, and CRM callback configuration.
2. Define `QuoteSession` or `ProjectSession` as the container for one customer interaction and multiple configured windows.
3. Define `ConfiguredWindowItem` or `ConfiguratorSession` as one configured window inside a quote container.
4. Define status transitions such as Draft, Completed, Submitted, and possibly Expired.
5. Define where product line lives by default: quote-level input with tenant policy controlling whether mixed product lines are allowed.

### Deliverables

1. Minimal C# models or DTO contracts.
2. A short ADR or design note describing CRM-launched and website-launched flows.
3. A clearly defined authoritative payload shape for completion.

### Why this comes second

This is the lowest-effort work that prevents major rework later. Without these definitions, pricing, persistence, and API design will drift.

## Phase 2: Introduce Session Persistence ✅ Complete

Risk reduced: loss of state, inability to support draft/resume, inability to support multi-window website flow.
Effort: medium.

### Goals

- Persist real configurator state.
- Make the application capable of more than canned demo data.

### Changes

1. Add durable storage for quote sessions and configured items.
2. Start with a pragmatic persistence model: a session record plus JSON payload is acceptable.
3. Persist key fields separately for querying: tenant id, external ids, product line key, status, authoritative price, timestamps.
4. Add draft save and draft load capability.

### Deliverables

1. Persistence model and repository or service layer.
2. Ability to create a new session and resume an existing one.
3. Ability to support multiple configured windows inside a quote container.

### Why this phase is early

Persistence is foundational for both serious CRM integration and the website prospect flow.

## Phase 3: Move Catalog Resolution Behind The Server ✅ Complete

Risk reduced: wrong template selection, fragile product-line behavior, client-controlled catalog access.
Effort: medium.

### Goals

- Stop treating templates and pricing files as arbitrary client-driven lookups.
- Make the server decide which catalog assets apply.

### Changes

1. Introduce a server-owned catalog manifest keyed by product line.
2. Resolve item templates, section templates, pricing files, assets, and option sets server-side.
3. Enforce tenant-level catalog access and mixed-product-line policy.
4. Replace hardcoded placeholder selection logic with session-driven selection.

### Deliverables

1. Catalog resolver service.
2. Manifest structure for product lines and related assets.
3. Updated controller or API logic that resolves catalog by session context instead of hardcoded filenames.

### Why this phase comes before pricing port

Authoritative pricing depends on authoritative catalog selection. If the wrong product line can still be loaded, server-side pricing is only partially trustworthy.

## Phase 4: Port Authoritative Pricing To The Server ✅ Complete

Risk reduced: client-side price tampering, inconsistent totals, inability to trust submitted amounts.
Effort: medium to high.

### Goals

- Make the server compute final price from submitted configuration.
- Keep live browser pricing only as an estimate.

### Changes

1. Port the current JavaScript pricing engine into a C# pricing service.
2. Reuse the same pricing data sources initially to reduce migration risk.
3. Compute per-item authoritative price server-side.
4. Compute quote-level aggregate totals server-side.
5. Persist server-calculated amounts and return them as authoritative.

### Deliverables

1. C# pricing service.
2. Tests covering the interpolation and pricing rules.
3. Completion flow that ignores client-submitted final price.

### Why this is the highest-value business phase

This is the first phase that makes the system commercially trustworthy.

## Phase 5: Add Server-Side Validation At Completion ✅ Complete

Risk reduced: invalid configurations being accepted, inconsistent catalog usage, pricing computed from unsupported states.
Effort: medium to high.

### Goals

- Re-check business rules at the server boundary.
- Keep browser validation for UX, but stop trusting it.

### Changes

1. Validate frame size restrictions server-side.
2. Validate section size restrictions and style compatibility server-side.
3. Validate product-line compatibility and option availability server-side.
4. Reject unsupported or inconsistent configuration payloads before completion.

### Deliverables

1. Validation service.
2. Error response shape the client can consume.
3. Test coverage for invalid completion scenarios.

### Why this follows pricing

Pricing and validation should share the same authoritative catalog context. Porting them together is safer than trying to validate against still-placeholder catalog logic.

## Phase 6: Add The Minimal API Surface ✅ Complete

Risk reduced: inability to integrate cleanly with CRM or website flows, reliance on ad hoc MVC endpoints.
Effort: medium.

### Goals

- Expose a small, clear, session-oriented API.
- Support both CRM-launched and website-launched workflows.

### Suggested Endpoints

1. `POST /api/v1/quote-sessions`
2. `GET /api/v1/quote-sessions/{id}`
3. `PUT /api/v1/quote-sessions/{id}`
4. `POST /api/v1/quote-sessions/{id}/items`
5. `PUT /api/v1/quote-sessions/{id}/items/{itemId}`
6. `POST /api/v1/quote-sessions/{id}/complete`

### Behavior

- CRM flow can create scoped sessions with external references and preselected product line.
- Website flow can create a quote container, add multiple items, and request a total.
- Completion returns normalized, authoritative output regardless of CRM schema differences.

### Why this phase waits until now

An API built before the session, catalog, pricing, and validation models stabilize will likely have to be redesigned.

## Phase 7: Add CRM Handoff And Tenant Integration Features ✅ Complete

Risk reduced: inability to operationalize completed quotes with clients.
Effort: medium to high.

### Goals

- Let the app hand off completed quote packages without becoming CRM-specific.
- Preserve a generic integration boundary.

### Changes

1. Store tenant callback configuration.
2. Emit normalized completion payloads.
3. Add webhook dispatch or equivalent callback mechanism.
4. Add delivery logging and failure tracking.
5. Add retry behavior when callbacks fail.

### Deliverables

1. Tenant integration settings.
2. Completion event payload contract.
3. Delivery tracking.

### Why this is later

A callback mechanism is only useful once the completed payload is authoritative and stable.

## Phase 8: Harden For Real Multi-Tenant Use ✅ Complete

Risk reduced: tenant leakage, wrong branding/catalog access, operational issues at scale.
Effort: high.

### Goals

- Make the system safe for multiple clients with different catalogs, brands, and policies.
- Support real website prospect flows per client.

### Changes

1. Add tenant branding configuration.
2. Add tenant-specific catalog access rules.
3. Add tenant-specific product-line policies.
4. Add tenant authentication and authorization strategy.
5. Add support for prospect identity and resume flows where needed.

### Why this is later

The earlier phases create the core trust boundary first. Tenant hardening should build on that core rather than complicating early implementation.

## Phase 9: Optional UI Port Or Technical Modernization 🚧 Active

Risk reduced: long-term maintainability, aging frontend stack.
Effort: high.

### Goals

- Improve maintainability without changing the proven interaction model.
- Port the existing UI only after the backend contracts are stable.

### Changes

1. Keep the current UX and workflows as the reference behavior.
2. Port the frontend to a newer stack only if it provides real maintenance benefits. (I think the benifit of porting pricing to c# and using blazor is good)
3. Consume the now-stable server APIs instead of duplicating authority in the client.

### Why this is last

This is valuable, but it does not meaningfully reduce the current product risk compared to server authority, persistence, and integration.

## Recommended Execution Order

If work needs to start immediately, do the next items in this order:

1. Complete Phase 0.
2. Produce the domain and session contracts from Phase 1.
3. Implement persistence from Phase 2.
4. Implement server-owned catalog resolution from Phase 3.
5. Port authoritative pricing from Phase 4. ✅ Complete
6. Add authoritative validation from Phase 5. ✅ Complete
7. Stabilize the session API from Phase 6. ✅ Complete
8. Add CRM handoff from Phase 7. ✅ Complete

## Definition Of A Meaningfully Real Product

The system becomes meaningfully real when all of the following are true:

1. A tenant can create or receive a quote session.
2. A user can configure one or more windows in that session.
3. The server persists the configuration state.
4. The server resolves the allowed catalog.
5. The server validates the final configuration.
6. The server computes authoritative per-item and aggregate pricing.
7. The system can hand the completed quote package to the tenant's CRM integration point.

## What Can Be Deferred Safely

These items are important, but they are not required to make the system trustworthy:

1. Full frontend rewrite.
2. Rich CRM-specific adapters.
3. Full normalized relational modeling of every configuration detail.
4. Advanced tenant self-service administration.
5. Perfect retry and queue infrastructure from day one.

## Summary

The highest-value path is backend-first, not frontend-first. The practical objective is to preserve the current configurator experience while replacing placeholder behavior with authoritative server services. The first milestone is not elegance. It is trust: persisted sessions, server-resolved catalog, server validation, and server pricing.
