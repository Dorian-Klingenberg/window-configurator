# Skill: vscode-debug-launch (Core Instructions)

This file is the single source of truth for the `vscode-debug-launch` skill. Runtime-specific wrappers can reference this file.

---

## Purpose

Set up and maintain reliable VS Code debug/task configurations for this workspace and branch so demo apps start consistently without stale-path or `uriFormat` failures.

This skill standardizes:
- `D:\Repos\renonerd\.vscode\launch.json`
- `D:\Repos\renonerd\.vscode\tasks.json`
- `D:\Repos\renonerd\WindowConfigurator\.vscode\launch.json`
- `D:\Repos\renonerd\WindowConfigurator\.vscode\tasks.json`

---

## When To Invoke

Invoke this skill when:
- a branch adds/removes a demo app (`MockContractorSite`, `MockContractorCrm`, etc.)
- VS Code debug fails with path errors (`MSB1009`, project file missing)
- VS Code shows `Format uri (...) must contain exactly one substitution placeholder`
- debug profiles are duplicated/confusing and need one stable “happy path”
- branch handoff requires reproducible debug startup

---

## Workspace Reality (Important)

Developers sometimes open either:
1. `D:\Repos\renonerd` (workspace container root), or
2. `D:\Repos\renonerd\WindowConfigurator` (repo root).

VS Code uses the `.vscode` folder from whichever is opened.  
Therefore keep both `.vscode` locations aligned for the active slice.

---

## Standard Configuration Rules

1. Keep one clearly named primary debug target per slice (for example `WindowConfigurator: Debug Slice B (CRM + Configurator)`).
2. Keep one configurator-only target (`WindowConfigurator: Debug Configurator`).
3. Pre-launch tasks must kill only the required ports for that slice.
4. `serverReadyAction.uriFormat` must contain **exactly one** `%s`.
   - valid: `"%s/"`, `"%s/00000000-0000-0000-0000-000000000001"`
   - invalid: `"http://localhost:5149/..."`
5. Use explicit app URLs via `ASPNETCORE_URLS` for deterministic ports.
6. Use consistent naming between root and repo `.vscode` files.
7. Do not leave stale project references to removed slice apps.

---

## Slice Mapping

Use this mapping unless the branch docs override it:

- Configurator API: `http://localhost:5149`
- Slice A host app (`MockContractorSite`): `http://localhost:5151`
- Slice B host app (`MockContractorCrm`): `http://localhost:5152`

If both host apps exist on a branch, use separate compound entries.

---

## Procedure

1. Detect active branch and existing app projects in `WindowConfigurator/`.
2. Detect which workspace root the user is running from (`renonerd` vs `WindowConfigurator`).
3. Update both `.vscode` locations to the same active-slice target set.
4. Validate JSON parse for each edited file.
5. Re-scan for stale app names/ports using ripgrep.
6. Run one debug-start smoke check (or provide exact debug profile to run if user is driving UI).
7. Document the change in the current journal entry when behavior changed.

---

## Verification Checklist

- `launch.json` parses in both locations.
- `tasks.json` parses in both locations.
- No stale references to inactive projects (e.g., `MockContractorSite` on Slice B branch).
- All `uriFormat` fields contain exactly one `%s`.
- Primary compound profile launches expected app pair for the active slice.

---

## Rules

- Do not edit files outside `D:\Repos\renonerd\`.
- Do not create more than one “primary” slice profile; keep debug options simple.
- Do not hardcode `uriFormat` as fixed URL strings.
- Do not remove a working configurator-only profile.
- Keep profile names descriptive and stable to reduce user confusion.

