# ADR 0015 — Mock Demo Host Architecture

**Date:** 2026-05-22
**Status:** Accepted

---

## Context

ADR 0002 defined two integration flows: CRM-initiated and website-initiated. Both flows require a host system — a CRM or a dealer website — to initiate a quote session and embed the configurator. Until Phase 10, no runnable host system existed for demo purposes. Demos required either a live CRM connection or a manually assembled URL, both of which are fragile and slow to set up.

The risk is presentation risk and adoption risk: if a demo requires a live CRM, the demo fails when the CRM is unavailable, slow, or mis-configured. If the integration story is explained verbally but not demonstrated, prospects cannot evaluate it.

---

## Decision

Build two standalone mock host applications as separate ASP.NET Core projects that live in the same repository as the Configurator:

**MockContractorSite** (port 5151)
- Models the website-initiated flow (ADR 0002, Flow 2)
- Static frontend served by Kestrel
- Single server-side endpoint: `POST /api/session-launch`
- Session bootstrap happens server-side using `QuoteSessionBootstrapClient`
- Hosts fixed CTA copy variants (A/B/C) for controlled copy experiments

**MockContractorCrm** (port 5152)
- Models the CRM-initiated flow (ADR 0002, Flow 1)
- Minimal API backend with an in-memory opportunity store (`InMemoryCrmOpportunityStore`)
- Manages a list of opportunities with lifecycle states (created → quote started → quote submitted)
- Session creation happens server-side using `CrmQuoteSessionClient`

Both applications:
- Authenticate with the Configurator API using `X-Api-Key` (ADR 0011)
- Never call any external CRM API (ADR 0001 constraint applies here too)
- Run as separate processes from the Configurator, with separate launch/task profiles in `.vscode/`

---

## Alternatives Considered

**Integrate demo surfaces into the Configurator project itself (rejected)**
Add demo routes and views directly to `WindowConfigurator.Web`. Rejected because it blurs the boundary between the platform and its clients, makes the repo harder to understand, and couples demo-only concerns to production code. A CRM is a separate system; a mock CRM should be a separate project.

**Single mock host app for both flows (rejected)**
Combine site and CRM simulation into one application. Rejected because the two flows have different entry-point mechanics, authentication models, and UI concerns. Keeping them separate makes it easy to demonstrate each flow independently and to grow each mock without interference.

**Use a real CRM in a sandbox account (rejected)**
Configure a HubSpot or Salesforce sandbox as the demo host. Rejected for Phase 10 because it reintroduces external dependency risk (sandbox rate limits, API key management, sandbox resets). A local mock can be reset deterministically and requires no external account.

**Browser-only demo (no server-side mock) (rejected)**
Simulate the host behavior entirely in browser JavaScript. Rejected because it would expose the Configurator API key in the browser — violating the API key contract — and because server-side session creation is part of the integration pattern being demonstrated.

---

## Consequences

- The two-flow architecture (ADR 0002) is now demonstrable end-to-end without a live CRM.
- Each mock app is a realistic protocol-accurate caller: it uses the same `X-Api-Key` header, the same `POST /api/v1/quote-sessions` endpoint, and the same session URL returned from that call.
- The mock apps are demo tooling, not production code. They are excluded from production deployment decisions.
- Both apps share the solution file and are built together by `dotnet build`, so compilation regressions are caught immediately.
- VS Code launch profiles in both `.vscode/` roots keep both flows launchable without manual port setup.
