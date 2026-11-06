# Handoff — 2026-05-22 (Slice B: Mock CRM Portal)

Source branch: `feature/phase10-ui-slice-b-mock-crm-portal`

## Current Truth Snapshot

- Repo root remains: `D:\Repos\renonerd\WindowConfigurator`
- Phases 0–9.5 are complete.
- Slice A is complete (see `HANDOFF-2026-05-22-slice-a.md`).
- Slice B code is complete but left the session with an unresolved debug experience issue.

## Slice B Progress

- `MockContractorCrm` app scaffolded and added to solution.
- In-memory opportunity endpoints added:
  - `POST /api/opportunities`
  - `GET /api/opportunities`
  - `POST /api/opportunities/{id}/start-quote`
- `CrmQuoteSessionClient` added to call real `POST /api/v1/quote-sessions` with API-key auth.
- Mock CRM UI now supports:
  - create opportunity
  - start quote
  - open returned configurator launch URL
- Test added and passing: `CrmQuoteSessionClientTests`.

## Expected Ports (Slice B)

- Configurator API: `5149`
- Mock CRM: `5152`
- Note: `5151` is the Slice A host-site port, not Slice B.

## Cross-Branch Follow-Up (Open)

Shared debug/tooling standardization introduced on Slice B:
- `skills/vscode-debug-launch-core.md`
- `AGENTS.md` skill registration
- branch-local `.vscode` profile normalization (both inner `WindowConfigurator/.vscode/` and outer `renonerd/.vscode/`)

Must be propagated to remaining active branches.
Tracking source of truth: `BRANCHING.md` → `Cross-Branch Rollout Checklist (Shared Tooling/Docs)`.

Current rollout status:
- Applied on `feature/phase10-ui-slice-b-mock-crm-portal` — verification still open
- Pending on `main`, Slice A, Slice C, Slice D branches

## Debug Blocker At Pause

- Chrome auto-open from VS Code debug profiles was not working reliably in the user environment.
- Manual port checks for `7049`, `5151`, `7151` also returned no response.
- The branch has explicit watcher-based Chrome launch tasks but behavior was not confirmed.
- The `uriFormat` fix (`%s` placeholder) is the correct fix — verify this resolves auto-open on resume.

## Next Step On Slice B Resume

1. Confirm Chrome auto-open is working (or accept manual open as sufficient).
2. Run `CrmQuoteSessionClientTests` to verify tests still pass.
3. Close out Slice B and open a PR into main.
4. Then begin Slice C (`feature/phase10-ui-slice-c-completion-confirmation`).
