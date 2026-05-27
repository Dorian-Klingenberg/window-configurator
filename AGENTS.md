# WindowConfigurator Agent Bootstrap

This directory is the real project root:

- Git repository root
- `WindowConfigurator.sln` location
- `implementation-roadmap.md` location
- `adr/` and `journal/` location

If you started from `D:\Repos\renonerd`, `cd WindowConfigurator` before running Git, build, test, or roadmap commands.

**Scope constraint:** All work stays within the `D:\Repos\renonerd\` workspace. The outer `renonerd/` folder contains configuration and tooling that may need to be edited. Do not read, edit, create, or delete anything outside `D:\Repos\renonerd\` — other repos in `D:\Repos\` are unrelated projects.

Read `../AGENTS.md` for Dorian's full working preferences, then read:

1. `implementation-roadmap.md`
2. `window-domain-knowledge.md`
3. the most recent file in `journal/`
4. any relevant ADRs in `adr/`
5. `BRANCHING.md`
6. All `HANDOFF-*.md` files in this directory — including branch-specific ones (e.g. `HANDOFF-2026-05-22-slice-a.md`). Each active feature branch may have its own handoff with progress state not yet on main. Run `git branch -a` and `git show <branch>:HANDOFF-*.md` to check branches for handoff docs that have not been merged yet.

Current phase discipline still applies: verify prior phases and tests before starting a new phase, and use TDD for non-trivial implementation work.

At each phase handoff (closeout or kickoff), do a quick documentation sanity check:
- roadmap phase status lines
- latest journal accuracy
- any same-day ADR consistency
- README/test-note freshness (counts, commands, paths)
- if branch focus changed, latest journal + handoff reflect the new priority

Keep this quick unless the user explicitly asks for a deep documentation review.

At each phase handoff, also create or update lessons in `lessons/`:
- maintain one lesson per phase touched and keep `lessons/README.md` current
- include actionable build steps, code snippets, and at least one Mermaid diagram per lesson
- run a quick consistency check so lessons match roadmap/ADRs/journal and current endpoint/code names

When asked for current project status, phase, or progress:
- do not answer from memory/context alone
- verify against `implementation-roadmap.md`, latest `journal/` entries, and recent ADRs/handoff docs
- report the documented truth; if documents conflict, resolve and state that before answering

## Available Skills

### create-lesson

**Purpose:** Produces a structured lesson document from completed phase, slice, or step work.

**When to invoke:** After completing any phase, slice, or step where new files were created, tests were written, and at least one design decision was made.

**How to invoke:**
- Claude Code: `/create-lesson <phase or step name>`
- Copilot: activate the `CreateLesson` agent
- Codex / any agent: read `skills/create-lesson-core.md` and follow its instructions for the described work

**Core instructions:** `skills/create-lesson-core.md`

**What it produces:** A `lessons/phase-N-title.md` file with build steps, a Mermaid diagram, representative code snippets, and a test table. Updates `lessons/README.md`.

### hubspot-org-config

**Purpose:** Applies declarative HubSpot organization configuration from source-controlled JSON using a Python tool with dry-run and apply modes.

**When to invoke:** When setting up or updating HubSpot CRM metadata (currently custom property upserts) for demo/integration environments, especially contractor-initiated demo flows.

**How to invoke:**
- Codex / any agent: read `skills/hubspot-org-config-core.md` and follow its workflow
- Run dry-run first:
  - `python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --dry-run`
- Then apply:
  - `python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --apply`

**Core instructions:** `skills/hubspot-org-config-core.md`

**What it produces:** HubSpot config action output in console plus JSON report (`artifacts/hubspot-org-config-report.json` by default), plus a minimal contractor-demo runbook (multi-item edit/save cycle and multi-order, multi-product-line flow).
