# Handoff — 2026-05-22 (Slice A: Mock Contractor Site)

Source branch: `feature/phase10-ui-slice-a-contractor-landing-shell`

## Current Truth Snapshot

- Repo root remains: `D:\Repos\renonerd\WindowConfigurator`
- Phases 0–9.5 are complete.
- Phase 10 (UI/customer-facing modernization) is now the active planning/implementation direction.
- HubSpot live testing is temporarily deferred due tenant responsiveness issues; HubSpot script tooling is complete enough to resume later without rework.

## Slice Planning Locks

- Two separate apps (mock contractor website + mock CRM portal), not one combined app.
- CTA testing starts with fixed variants.
- Demo reset creates a new quote session every run.
- No Blazor modernization in Slice A–D scope.

## Slice A Progress

- `MockContractorSite` app scaffolded and added to solution.
- Fixed CTA variants (A/B/C) added on landing page.
- `POST /api/session-launch` implemented to create quote sessions with API-key auth.
- Iframe launch wired from host app to configurator session URL.
- Test added and passing: `QuoteSessionBootstrapClientTests`.
- VS Code debug profiles now include a temporary prelaunch DB reset task to avoid stale SQLite schema crashes during Slice A branch work.
- VS Code `uriFormat` placeholder issue resolved (`%s/...`) and both `.vscode` roots aligned to avoid mismatched debug options.

## Scope Boundary Decision

- Multi-window quote-item creation/continuation UX is explicitly assigned to the configurator application layer, not the mock contractor website.
- Rationale: backend already supports multi-item sessions; host site should remain orchestration-only to avoid duplicating quote workflow logic.

## Slice A Closeout Note

- Single-tab debug behavior is accepted for Slice A.
- Multi-tab debug auto-open polish is deferred and can be revisited in Slice D.

## Expected Ports (Slice A)

- Configurator API: `5149`
- Mock Contractor Site: `5151`
