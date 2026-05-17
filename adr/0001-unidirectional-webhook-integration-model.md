# ADR 0001 — Unidirectional Webhook Integration Model

**Date:** 2026-05-11
**Status:** Accepted

---

## Context

WindowConfigurator needs to hand off completed configuration data to the window dealer's CRM (Salesforce, HubSpot, or any other system the dealer uses). The naive approach would be to write CRM-specific adapters — one for Salesforce, one for HubSpot, etc. — that call those APIs directly after a session completes.

This was evaluated and rejected.

The platform is intended to be sold to multiple window dealers. Each dealer may use a different CRM, or switch CRMs over time. Writing and maintaining CRM-specific adapters creates an ongoing maintenance burden, requires API credentials and knowledge for each CRM, and tightly couples the platform to each client's sales tooling.

Additionally, CRM integration is typically owned by a CRM specialist on the client side — someone who knows their system's field mappings, workflow automation, and data model. That expertise does not live in this platform.

---

## Decision

**WindowConfigurator never calls any CRM API. It fires a generic outbound webhook.**

When a quote session is completed, the platform POSTs the configured item payload to a URL that the tenant registered at setup time. That URL belongs to the dealer's CRM, a middleware layer, or any other system they want to receive the event.

The CRM side owns interpretation. The webhook consumer decides what to do with the payload — create a lead, update an opportunity, trigger a workflow. WindowConfigurator has no knowledge of and no dependency on what that system is.

The tenant registers their callback URL once. Everything else is their problem.

---

## Alternatives Considered

**CRM-specific adapters (rejected)**
Write a Salesforce adapter, a HubSpot adapter, etc. Rejected because it creates a permanent maintenance burden, requires API credentials for each CRM, and puts CRM domain knowledge inside a platform that should not have it. Every new CRM integration becomes a product feature request.

**Polling / pull model (rejected)**
CRM polls the API for completed sessions. Rejected because it requires the CRM to manage poll frequency, introduces latency, and creates unnecessary load. Push is the right model for an event-driven handoff.

**Direct database access (rejected)**
Share a database the CRM can read. Rejected immediately — this is not a real integration pattern and violates every tenant boundary.

---

## Consequences

- The platform is fully CRM-agnostic. Adding a new client with a different CRM requires zero platform changes.
- The tenant's CRM admin owns their integration entirely. This is a feature, not a gap.
- The platform must implement reliable webhook dispatch: delivery tracking, retry with exponential backoff, and failure logging. A failed delivery must not be silently lost.
- The completion payload must be stable and well-documented, because downstream consumers will build against it.
- The platform has no visibility into what the CRM does with the payload after delivery. This is intentional.
