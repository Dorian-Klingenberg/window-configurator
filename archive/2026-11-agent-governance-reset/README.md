# Agent Governance Reset Archive

## Purpose

This archive preserves the repository's pre-reset agent governance without
leaving it active. It records the process that guided earlier work, the
historical branch and handoff context, and the migration plan for replacing
that process through deliberate practice with the installed agents.

The archived instruction files are complete, unedited copies from their former
active paths. Their SHA-256 values are recorded in [MANIFEST.md](./MANIFEST.md)
so the reset remains auditable.

The archived files are historical evidence. They are not fallback instructions
for future agents.

## Active agent surface after the reset

Only the following installed bundles remain active:

| Retained path | Role |
|---|---|
| `.github/agents/ada.agent.md` | Discovery, design discussion, and implementation handoffs |
| `.github/agents/certification-learning-coach.agent.md` | GH-600, AI-103, and AI-500 learning within its `training/` boundary |
| `.github/skills/engineering-method/` | Ada's explicitly invoked reference material |

No root bootstrap file, cross-agent wrapper, legacy skill registry, or stale
branch policy remains in the active repository surface.

## Archived material

| Previous active location | Archive location | Reason |
|---|---|---|
| `AGENTS.md` | `legacy-agent-instructions/AGENTS.md` | Superseded global agent constitution |
| `CLAUDE.md`, `GEMINI.md`, `.cursorrules` | `legacy-agent-instructions/` | Duplicated bootstrap instructions for older agent hosts |
| `.claude/` | `legacy-agent-instructions/.claude/` | Legacy command wrappers and host configuration |
| `skills/` | `legacy-agent-instructions/skills/` | Legacy project-wide skill registry and instructions |
| `.github/copilot-instructions.md`, `.github/CreateLesson.agent.md` | `legacy-agent-instructions/.github/` | Copilot bootstrap and legacy custom-agent wrapper |
| `BRANCHING.md` | `legacy-governance/BRANCHING.md` | Superseded lane policy that referenced retired branches |
| `HANDOFF-*.md` | `legacy-governance/` | Historical May 2026 handoffs, not current operating state |

The separate [legacy Agile V and workflow archive](./legacy-agile-v/README.md)
contains the historical methodology corpus and its original directory layout.

Product code, architecture decisions, the active product direction, current
status, and the two retained agent bundles were not moved. Historical
requirements, roadmap, journals, lessons, metrics, sandboxes, and mixed
discovery records are preserved in `legacy-agile-v/`.

## How to use this archive

Read an archived file only to understand historical context or recover a
specific decision. Do not restore a file wholesale. A future practice may be
reintroduced only after it has been evaluated against the plan in
[GH-600 migration plan](./GH-600-MIGRATION-PLAN.md).
