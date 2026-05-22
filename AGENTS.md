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

Current phase discipline still applies: verify prior phases and tests before starting a new phase, and use TDD for non-trivial implementation work.

At each phase handoff (closeout or kickoff), do a quick documentation sanity check:
- roadmap phase status lines
- latest journal accuracy
- any same-day ADR consistency
- README/test-note freshness (counts, commands, paths)

Keep this quick unless the user explicitly asks for a deep documentation review.

At each phase handoff, also create or update lessons in `lessons/`:
- maintain one lesson per phase touched and keep `lessons/README.md` current
- include actionable build steps, code snippets, and at least one Mermaid diagram per lesson
- run a quick consistency check so lessons match roadmap/ADRs/journal and current endpoint/code names

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
