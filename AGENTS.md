# WindowConfigurator Agent Bootstrap

This directory is the real project root:

- Git repository root
- `WindowConfigurator.sln` location
- `implementation-roadmap.md` location
- `adr/` and `journal/` location

If you started from `D:\Repos\renonerd`, `cd WindowConfigurator` before running Git, build, test, or roadmap commands.

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
