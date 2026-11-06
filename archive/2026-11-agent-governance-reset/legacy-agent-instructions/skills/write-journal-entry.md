# Skill: write-journal-entry (Core Instructions)

This file is the single source of truth for the `write-journal-entry` skill.

---

## Purpose

Write or update a journal entry that records what happened in a working session — decisions made, work completed, current state, and what the next agent needs to know.

---

## When To Use

At the end of any working session where:
- Code was written, tests were run, or designs were decided
- Architectural choices were made
- Project state changed in any material way

Also write a kickoff entry at the start of a new phase or slice to record the starting state.

---

## File Location and Naming

```
journal/YYYY-MM-DD.md
```

One file per calendar day. If an entry for today already exists, append to it rather than creating a second file. Use a horizontal rule `---` to separate session blocks if multiple sessions share a day.

---

## Structure

### Title

```markdown
# Journal — [Month Day, Year]
```

Example: `# Journal — June 11, 2026`

### Active Branch Block (when branch-specific)

Place this at the very top when the session is working on a feature branch:

```markdown
## Active Branch

`feature/phase10-ui-slice-d-demo-ux-hardening`

[One sentence on the branch state — e.g., "PR #4 not yet open. Implementation in progress."]
```

### Required sections (in order)

1. **`## What Happened Today`** — narrative prose, 1–3 paragraphs. The main through-line: what was this session actually about? Write it so a future reader can understand the session without reading all the sub-sections.

2. **Work stream sections** — one `##` section per distinct work stream or topic area covered. Use descriptive headings: `## Agile V First Component`, `## ADR Catch-Up`, `## Branch Sync`. Prose in these sections, not bullets.

3. **`## Current State`** — table showing the active work items and their state.

4. **`## Before Starting Work` or `## Notes For Incoming Agent`** — brief, actionable bullets covering what the next agent needs to know before touching anything.

### Optional sections

- **`## Validation Performed`** — include when tests were run, builds verified, or manual smoke tests done. Include exact commands in code blocks.
- **`## Ports Reference`** or **`## Environment Notes`** — include when port mappings, env vars, or tooling changed.

---

## Section rules

**`## What Happened Today` and work stream sections: prose.**
Write in narrative style, past tense for what was done, present tense for decisions and current state. Explain the *why* behind decisions, not just the *what*. If something was ruled out, say so and say why.

**`## Current State` table:**

| Item | State |
|---|---|
| Phase 10 Slice D | ⬜ Not started |
| PR #3 | 🟡 Open, ready to merge |
| main push | ⚠️ 2 commits pending — run manually |

Status icons: ✅ Complete · 🟡 In progress / waiting · ⬜ Not started · ⚠️ Requires manual action · ❌ Blocked

**`## Validation Performed` bullets:**
```markdown
- Rebuilt target: `cmake --build build --target grannys_house_trials_grass_field_003`
- Ran tests: `dotnet test` — 203 passing, 0 failing
- Smoke-launched: confirmed no startup crash
```

**`## Notes For Incoming Agent` bullets:**
Concrete actions and constraints. Write as if briefing someone who has never seen this codebase. State blockers, prerequisites, and any "do not do X until Y" constraints explicitly.

---

## What to capture in work stream sections

- What was decided and why (not just what was done)
- What was considered and ruled out, and why
- Any constraint or blocker that arrived during the session
- Any state that will surprise a future reader
- Any manual step that must be done by the human (not by an agent)

---

## Style rules

- Prose sections: narrative, past tense for actions, present tense for state
- No filler ("we made great progress today")
- Be specific: name files, commands, decisions
- If a constraint applies to future agents, state it as a direct instruction
- No emoji in prose — only in the Current State table's state column
- No trailing summaries after the Notes section

---

## Acceptance Checks

- Does `## What Happened Today` explain the session without requiring sub-sections to be read?
- Does `## Current State` reflect the actual state of all active work items?
- Are all manual-action requirements visible in `## Notes For Incoming Agent`?
- Are all build/test commands in `## Validation Performed` exact and runnable?
