---
name: write-journal-entry
description: Writes or updates the daily journal entry for the current working session. Records decisions made, work completed, current state, and notes for the next agent.
argument-hint: (optional) additional context or specific topics to cover in this entry
---

Read `skills/write-journal-entry.md` for the full instruction set.

Apply those instructions to write or update `journal/{{now | date: "%Y-%m-%d"}}.md` for the current session. {{args}}
