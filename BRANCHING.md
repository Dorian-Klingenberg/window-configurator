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

## Active Branch Briefs

### `feature/phase10-ui-slice-a-contractor-landing-shell`

Builds the mock contractor website landing experience for customer demos.

Planned content:
- branded landing shell
- primary CTA into configurator flow
- host-side launch strategy for configurator (iframe preferred, redirect fallback)

Success outcome:
- deterministic demo entrypoint with no dependency on live CRM responsiveness.

### `feature/phase10-ui-slice-b-mock-crm-portal`

Builds a mock contractor CRM portal surface to simulate contractor workflow context.

Planned content:
- mock record/context view
- quote/session start controls
- launch into configurator with relevant session context

Success outcome:
- realistic CRM-initiated narrative for demos without external CRM dependency.

### `feature/phase10-ui-slice-c-completion-confirmation`

Builds completion return/confirmation surfaces in host experience.

Planned content:
- authoritative completion summary
- session/item identifiers
- authoritative price visibility and submission status cues

Success outcome:
- clear customer-facing evidence of successful end-to-end completion flow.

### `feature/phase10-ui-slice-d-demo-ux-hardening`

Hardens the end-to-end demo UX for reliability and presenter confidence.

Planned content:
- loading/empty/error states
- retry messaging and recovery affordances
- mobile/responsive polish for host + launch flow

Success outcome:
- low-friction demo execution under variable local/network conditions.

### `feature/phase10-demo-host-flow-wiring`

Covers orchestration glue where host/demo wiring is shared or cross-cutting.

Planned content:
- environment/config switches for demo mode
- host-to-configurator parameter wiring
- non-UI flow integration points that do not belong in pure UI slices

Success outcome:
- stable orchestration foundation reused by UI and mock CRM surfaces.

### `feature/hubspot-tooling-private-app-bootstrap`

Maintains HubSpot org bootstrap/support automation as a non-blocking track.

Planned content:
- private-app-token-based setup docs/tooling
- repeatable property/bootstrap configuration
- deterministic dry-run/apply/report outputs

Success outcome:
- HubSpot setup can resume quickly when portal responsiveness is acceptable, without blocking Phase 10 demo progress.

## Documentation Requirements Per Branch Pivot

When priorities change between lanes, update all three:

1. `journal/YYYY-MM-DD.md`
2. latest `HANDOFF-YYYY-MM-DD.md` (or create a new same-day handoff)
3. `implementation-roadmap.md` execution note (if direction materially changed)
