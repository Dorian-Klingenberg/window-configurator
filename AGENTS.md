# WindowConfigurator Agent Bootstrap

This directory is the real project root:

- Git repository root
- `WindowConfigurator.sln` location
- `implementation-roadmap.md` location
- `adr/`, `journal/`, `lessons/`, `skills/` location

If you started from `D:\Repos\renonerd`, `cd WindowConfigurator` before running Git, build, test, or roadmap commands.

**Scope constraint:** All work stays within the `D:\Repos\renonerd\` workspace. Do not read, edit, create, or delete anything outside `D:\Repos\renonerd\` — other repos in `D:\Repos\` are unrelated projects.

---

## Required Reading Before Any Work

Read these in order before starting any task:

1. `implementation-roadmap.md` — active phase, deliverables, history
2. `window-domain-knowledge.md` — window industry domain reference
3. Most recent file in `journal/` — current state, recent decisions
4. All files in `adr/` — architectural decisions, constraints, rationale (0001–0018)
5. `BRANCHING.md` — active branch lanes and merge rules
6. All `HANDOFF-*.md` files in this directory — including branch-specific ones. Run `git branch -a` then `git show <branch>:HANDOFF-*.md` on each active feature branch to find handoff docs not yet merged to main.
7. Run `git branch -a` and `git log --oneline -10` — confirm which branch you are on before writing any code

---

## Environment & Tool Constraints

- Never use visual or GUI tools (such as File Explorer, native application windows, or browser screenshots) if a command-line alternative is available.
- Always prefer executing commands within Windows Subsystem for Linux (WSL) over any other interface.
- If a task cannot be handled within WSL, fall back to standard CLI tools or PowerShell commands.
- Use text-based terminal utilities (`ls`, `grep`, `find`, `cat`, `Get-ChildItem`) exclusively for navigating file systems and managing project tasks.

---

## Development Method

**Test-Driven Development is the default.** Every non-trivial implementation starts with a failing test.

The TDD cycle:
1. Write a test that references the thing you're about to build
2. Confirm it fails to compile or fails to run (red)
3. Implement the minimum code to make it pass (green)
4. Proceed

Do not write implementation code before its tests exist. This applies to repositories, services, controllers, and any new class with behavior.

---

## Phase Discipline

Work is organized into numbered phases defined in `implementation-roadmap.md`.

**Never start Phase N+1 while Phase N is incomplete.**

Before beginning any phase, verify the previous phase is fully done — all deliverables exist, all tests pass. Ask if unsure.

---

## Backend First

The configurator UI is mature and works. The backend is where the real work is.

Do not touch the frontend unless:
- The backend contract it depends on has changed, or
- The task explicitly requires a frontend change

**Make the server authoritative before touching the client.**

---

## Risk Reduction Ordering

When sequencing work within a phase, prioritize by **risk reduction first, elegance second**.

A change that removes a critical assumption or a hard-coded placeholder is more valuable than a change that improves code structure. Both matter, but not equally.

---

## Code Conventions

### Naming
- Entities: suffixed `Entity` — `TenantEntity`, `QuoteSessionEntity`
- Repository interfaces: prefixed `I` — `IQuoteSessionRepository`
- EF Core repository implementations: prefixed `Ef` — `EfQuoteSessionRepository`
- Application service interfaces: `I{Name}Service` — `ICatalogService`, `ITemplateReader`
- Application service implementations: match without `I` — `CatalogService`
- Product line keys: lowercase kebab-case — `"energysaver-2500"`, `"apex"`, `"carriage"`
- Test classes: named after the thing they test, suffixed `Tests`

### Project structure
- Domain entities: `WindowConfigurator.Data/Entities/`
- Repository interfaces and implementations: `WindowConfigurator.Data/Repositories/`
- Application services: `WindowConfigurator.Data/Services/`
- DbContext: `WindowConfigurator.Data/`
- Tests: organized by concern inside `WindowConfigurator.Tests/` — `Entities/`, `Infrastructure/`, `Repositories/`, `Services/`, `Controllers/`

### Async
All repository methods are async. Use `Task<T>` returns and `await` throughout. Do not mix sync and async paths.

### Comments
Only comment code that genuinely needs clarification. No XML doc comments on trivial properties or obvious constructors. Do add summaries to public interfaces and fields carrying non-obvious business rules.

---

## Testing Conventions

- **Framework:** xUnit
- **Integration tests use SQLite in-memory** — `new SqliteConnection("Data Source=:memory:")` + `EnsureCreated()`, NOT the EF Core InMemory provider. SQLite in-memory runs real SQL and catches query bugs the InMemory provider does not.
- **Tests must fail before the code exists.** A test that passes before its implementation is written is not testing anything.
- One behavior per test method. Assert one thing, or closely related things.
- Test method names: `Subject_Condition_ExpectedBehavior` or `MethodName_WhenCondition_Behavior`

---

## Architecture Principles

Non-negotiable:

1. **CRM-agnostic.** The platform never calls any CRM API. It fires generic outbound webhooks. See `adr/0001-unidirectional-webhook-integration-model.md`.
2. **Unidirectional.** The CRM calls us. We do not call the CRM.
3. **Server-authoritative.** Pricing, validation, and catalog resolution belong on the server, not the client.
4. **No floating point for measurements.** Window measurements use exact fractional arithmetic (`sign`, `whole`, `numerator`, `denominator`). A 1/16" error is a real manufacturing problem.
5. **Rough opening is null at quote time.** It is only populated after a contractor remeasure visit. A null rough opening means the item is not order-ready.
6. **One manufacturer purchase order per product line.** Mixed product lines in a quote session become multiple orders at conversion time.

---

## Repository Zones

The repo has four documentation/work zones with different rules:

| Directory | Zone | V-Model applies? | Required output |
|---|---|---|---|
| `sandboxes/` | Pre-Phase A — exploration | No | Lesson with "What This Tells the Project" chapter |
| `requirements/` | Phase A entry | Yes — this IS the gate | Baselined requirements doc before slice branch opens |
| production slices | Phase A–D — implementation | Yes | TDD, ADR, PR gate |
| `metrics/` | Measurement layer | N/A — observation only | Tier 1 baseline + per-slice Tier 2 records |

**Sandboxes:** V-Model-free space. Each sandbox gets `sandboxes/NNN-kebab-question/` with a README stating the question before code is written. The only exit artifact required is a lesson. A sandbox does not have to produce a requirement — understanding is a valid result. See `sandboxes/README.md`.

**Requirements:** Written before any implementation work begins on a slice. One file per slice (`requirements/slice-N-kebab-topic.md`). IDs are `REQ-X-NNN`. Once written, baselined for the slice. See `requirements/README.md`.

**Metrics:** Tracks the Agile V adoption experiment. `metrics/baseline.md` holds Tier 1 numbers from git history. Per-slice Tier 2 files accumulate as work completes. See `metrics/README.md`.

---

## Documentation Conventions

### ADRs
Significant architectural decisions go in `adr/` as numbered Markdown files:
- `0001-kebab-case-title.md`
- Format: Context → Decision → Alternatives Considered → Consequences
- Write ADRs the same day decisions are made.

### Journals
Work sessions are documented in `journal/YYYY-MM-DD.md`. Entries are narrative and retrospective — what happened, what was learned, and why. Not ADRs. Always include:
- **What** decision was made
- **Why** that decision was made (tradeoffs, constraints, risk reduction rationale)

### Lessons
At each phase handoff, create or update a lesson in `lessons/`. See `skills/create-lesson-core.md` for the format. Keep `lessons/LESSON_CATALOG.md` current.

### Domain knowledge
The window industry domain reference lives in `window-domain-knowledge.md`. Update it when domain knowledge is discovered or clarified during a session.

---

## What Not To Do

- Do not read, edit, create, or delete any files outside `D:\Repos\renonerd\`
- Do not call CRM APIs from this codebase — ever
- Do not store prices or measurements as floating point
- Do not add new linting, build, or test tooling unless the task explicitly requires it
- Do not rewrite the frontend configurator (Knockout.js + Bootstrap 3) — it works
- Do not create markdown planning files in the repo — keep planning in session state
- Do not silently skip the TDD step
- Do not start a new phase until the current one is verified complete

---

## Agent Interaction Patterns

### Incremental Approval
When beginning a new phase or multi-step task: implement one step, present it for review. If it lands well, implement all remaining steps in one go. The first step is the proof of concept; the rest follow automatically once the approach is approved.

### Batch Review
Once the full implementation of a phase is underway (post-approval), implement all steps, then present them all together. Do not interrupt mid-implementation for step-by-step approval.

### Terse Communication
Messages are short. Infer intent from context. If in doubt about scope, check `implementation-roadmap.md` and the most recent journal entry — the answer is usually there.

### Status Requests Mean Fresh Verification
When asked for "current status", "where are we", or similar: do not answer from session memory alone. Always verify against `implementation-roadmap.md`, latest `journal/` entry, relevant ADRs, and handoff docs. Report the documented truth. If docs conflict, call out the conflict and reconcile before answering.

### Phase Verification Before Starting
Before starting any new phase, explicitly verify the prior phase is complete: all deliverables exist, all tests pass. State this before proceeding.

### Documentation Sanity Check At Phase Transitions
When closing or starting a phase, run a quick check:
- Roadmap phase markers are current
- Latest journal reflects what actually shipped
- Same-day ADRs align with implementation and naming
- README/test-note counts, commands, and paths are not stale

Keep this lightweight. Only do a deep full-doc audit when explicitly requested.

---

## Available Skills

### create-lesson

**Purpose:** Produces a structured lesson document from completed phase, slice, step, experiment, or planning session.

**When to invoke:** After completing any phase, slice, or step where new files were created, tests were written, and at least one design decision was made. Also invoke after planning or framework-adoption sessions.

**How to invoke:**
- Claude Code: `/create-lesson <topic>`
- Copilot: activate the `CreateLesson` agent
- Codex / any agent: read `skills/create-lesson-core.md` and follow its instructions

**Core instructions:** `skills/create-lesson-core.md`

**What it produces:** A `lessons/YYYY-MM-DD_kebab-topic.md` file with numbered prose chapters, "What We Learned" bullet summary, "What Comes Next" list, both Mermaid diagrams, and optional Research References. Updates `lessons/LESSON_CATALOG.md`.

### write-journal-entry

**Purpose:** Writes or updates the daily journal entry for the current session.

**When to invoke:** At the end of any working session where code was written, decisions were made, or project state changed. Also at the kickoff of a new phase or slice.

**How to invoke:**
- Claude Code: `/write-journal-entry`
- Codex / any agent: read `skills/write-journal-entry.md` and follow its instructions

**Core instructions:** `skills/write-journal-entry.md`

**What it produces:** An entry in `journal/YYYY-MM-DD.md` with narrative summary, per-work-stream sections, Current State table, optional Validation Performed commands, and Notes for the incoming agent.

### hubspot-org-config

**Purpose:** Applies declarative HubSpot organization configuration from source-controlled JSON using a Python tool with dry-run and apply modes.

**When to invoke:** When setting up or updating HubSpot CRM metadata for demo/integration environments.

**How to invoke:**
- Dry-run: `python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --dry-run`
- Apply: `python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --apply`

**Core instructions:** `skills/hubspot-org-config-core.md`

### vscode-debug-launch

**Purpose:** Standardizes VS Code `launch.json`/`tasks.json` for this workspace so debug startup is repeatable across branches and slices.

**When to invoke:** When starting a new slice branch, when demo app projects/ports change, or when debug startup fails.

**Core instructions:** `skills/vscode-debug-launch-core.md`
