# Sandbox 001 — interview-me

**Question:** Can a structured agent-driven self-interview replace or compress Pre-Phase A for requirements the stakeholder already holds implicitly?

---

## Hypothesis

The `interview-me` skill's five-step process (Hypothesize → One question at a time → Listen for "should want" signals → Restate in user's words → Confirm explicitly) will surface the implicit requirements Dorian holds for a fuzzy scope area and produce a confirmed statement of intent that maps directly to REQ-X-NNN format — without requiring a full sandbox cycle first.

The skill's output structure (Outcome / User / Why now / Success criteria / Binding constraint / Out of scope) is close enough to a requirements baseline that the translation step should be mechanical, not creative.

---

## Time limit

One session. If the interview does not produce a confirmed statement of intent after two full cycles through the skill, the sandbox closes with a lesson.

---

## Definition of done

Run `interview-me` against a real fuzzy scope area — Slice D "demo UX hardening" is the candidate, since that requirements doc needs to be written before the slice branch opens.

One of three outcomes closes this sandbox:

1. **Confirmed intent maps to requirements** — the statement of intent translates to 3–5 REQ-D-NNN statements with minimal interpretation. `interview-me` earns a slot in the Pre-Phase A / requirements toolkit.
2. **Useful but incomplete** — the skill surfaces real constraints and goals but still requires a sandbox or prototype to resolve key unknowns. Lesson records what `interview-me` can and cannot replace.
3. **Does not help** — the structured interview stalls or produces generic output that doesn't advance the requirements. Lesson explains why and whether a different framing would change the result.

---

## Skill source

`https://github.com/addyosmani/agent-skills/blob/main/skills/interview-me/SKILL.md`

Copy the skill instructions into this sandbox directory if running the experiment requires modifying or annotating them.

---

## What to record

During the session, note:
- Which questions broke through vs. which produced "should want" signals
- Whether the stop condition ("can I predict the user's reaction to the next three questions?") was reachable
- The confirmed statement of intent verbatim, if produced
- How many manual edits were needed to convert it to REQ-D-NNN format

These observations go into the lesson's "What We Learned" chapter.
