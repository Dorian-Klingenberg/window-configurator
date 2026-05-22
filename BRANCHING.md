# Branching Strategy

This file defines branch lanes and merge rules for ongoing WindowConfigurator work.

## Goals

1. Keep `main` stable and demoable.
2. Isolate higher-risk UI/customer-facing changes from backend authority work.
3. Keep HubSpot/support experimentation non-blocking.

## Long-Lived Branch Lanes

- `main`
  - Stable integration branch.
  - Backend-authoritative baseline (Phases 0–9.5 complete).

- `feature/phase10-ui-*`
  - Customer-facing UI modernization and UX hardening.
  - Includes mock contractor website and mock CRM portal surfaces.

- `feature/phase10-demo-*`
  - Demo orchestration and host-flow wiring where separation from UI branch is useful.
  - Optional lane if work is not directly in UI modernization files.

- `feature/hubspot-tooling-*`
  - HubSpot automation/support tooling.
  - Must not block customer-facing demo progress.

## Naming Conventions

- `feature/phase10-ui-<slice>-<short-desc>`
- `feature/phase10-demo-<slice>-<short-desc>`
- `feature/hubspot-tooling-<short-desc>`
- `fix/<area>-<short-desc>`
- `docs/<topic>-<yyyymmdd>`

Examples:
- `feature/phase10-ui-slice-a-contractor-landing-shell`
- `feature/phase10-ui-slice-b-mock-crm-portal`
- `feature/phase10-ui-slice-c-completion-confirmation`
- `feature/hubspot-tooling-private-app-bootstrap`

## Merge Rules

1. Merge only through PRs into `main`.
2. Keep PRs small and slice-based.
3. Include test updates when behavior changes.
4. Include documentation updates (journal/handoff/roadmap notes) when branch focus or architecture decisions change.
5. Prefer one active merge target at a time to reduce cross-branch drift.

## Current Recommended Order

1. `feature/phase10-ui-slice-a-contractor-landing-shell`
2. `feature/phase10-ui-slice-b-mock-crm-portal`
3. `feature/phase10-ui-slice-c-completion-confirmation`
4. `feature/phase10-ui-slice-d-demo-ux-hardening`

HubSpot lane continues in parallel when available:
- `feature/hubspot-tooling-*`

## Documentation Requirements Per Branch Pivot

When priorities change between lanes, update all three:

1. `journal/YYYY-MM-DD.md`
2. latest `HANDOFF-YYYY-MM-DD.md` (or create a new same-day handoff)
3. `implementation-roadmap.md` execution note (if direction materially changed)
