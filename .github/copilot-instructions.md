# GitHub Copilot Instructions — WindowConfigurator

Read these two files first. They contain the full working preferences, TDD rules, code conventions, architecture principles, and documentation requirements:

1. `AGENTS.md` (repo root) — project-specific bootstrap and skill registry
2. `../AGENTS.md` — Dorian's full working preferences (TDD, backend-first, naming, testing, interaction patterns)

Do not start any task before reading both.

---

## Required Reading Before Any Work

After reading the two AGENTS files above, read these in order:

1. `implementation-roadmap.md` — active phase, deliverables, history
2. `window-domain-knowledge.md` — window industry domain reference
3. Most recent file in `journal/` — current state, recent decisions
4. All files in `adr/` — architectural decisions, constraints, and rationale that govern the codebase
5. `BRANCHING.md` — active branch lanes and merge rules
6. All `HANDOFF-*.md` files in this directory — including branch-specific ones
7. Run `git branch -a` and `git log --oneline -10` — confirm which branch you are on before writing any code

---

## Environment & Tool Constraints

- Never use visual or GUI tools (such as File Explorer, native application windows, or browser screenshots) if a command-line alternative is available.
- Always prefer executing commands within Windows Subsystem for Linux (WSL) over any other interface.
- If a task cannot be handled within WSL, fall back to standard CLI tools or PowerShell commands.
- Use text-based terminal utilities (e.g., `ls`, `grep`, `find`, `cat`, `Get-ChildItem`) exclusively for navigating file systems and managing project tasks.

## Non-Negotiable Rules (Short Form)

- **TDD always.** Write the failing test before writing implementation code.
- **Backend first.** Don't touch the frontend unless the backend contract changed or the task explicitly requires it.
- **No CRM API calls from this codebase. Ever.**
- **No floating-point for prices or measurements.**
- **Verify the current branch before starting.** Check `git branch` — do not assume you are on main.
- **Do not work outside `D:\Repos\renonerd\`.** Other repos in `D:\Repos\` are unrelated.

Full rules and rationale: `../AGENTS.md`

---

## Available Custom Agents

- **CreateLesson** — invoke after completing any phase, slice, or planning session to produce a structured lesson document. See `skills/create-lesson-core.md`.
- **WriteJournalEntry** — invoke at the end of any working session to write or update the daily journal entry. See `skills/write-journal-entry.md`.
