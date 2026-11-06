# Sandbox 002 — idea-refine

**Question:** Can `idea-refine` bridge the gap between a fuzzy project initiative and a baselined requirements document — specifically by producing a "Not Doing" list and MVP scope that prevent the scope drift that makes requirements hard to write?

---

## Hypothesis

`idea-refine`'s three-phase process (Divergent → Convergent → Sharpen & Ship) produces a Phase 3 one-pager with: problem statement, recommended direction, key assumptions to validate, MVP scope, and a "Not Doing" list. That last artifact — explicit scope exclusions with reasoning — is what's been missing from the slice requirements format. Without a "Not Doing" list, requirements drift because there is no written boundary.

The hypothesis: feeding a real fuzzy initiative through `idea-refine` will produce a Phase 3 document whose MVP scope and "Not Doing" list are directly usable as the skeleton of a requirements baseline, with the REQ-X-NNN statements being the final mechanical step.

---

## Time limit

One session. If Phase 3 does not produce a usable scope boundary after one full pass through the skill, the sandbox closes.

---

## Definition of done

Run `idea-refine` against a real initiative that needs requirements. Two candidates:

1. **Slice D "demo UX hardening"** — concrete enough to test the skill's convergent phase.
2. **The Agile V adoption experiment itself** — fuzzier, which would stress-test whether the skill handles meta-level product questions.

Pick one (or run both if time allows).

One of three outcomes closes this sandbox:

1. **Phase 3 maps to requirements** — the MVP scope + "Not Doing" list translates to REQ-X-NNN statements + scope exclusion section with minimal interpretation. `idea-refine` earns a slot at the Phase A entry gate.
2. **Useful intermediate artifact** — the one-pager adds structure but still requires `interview-me` or a sandbox to resolve unknowns before requirements can be written. Lesson records the two-skill sequence.
3. **Does not help** — the divergent phase produces too much noise, or the convergent phase stalls on genuinely unresolved questions. Lesson explains the failure mode.

---

## Skill source

`https://github.com/addyosmani/agent-skills/blob/main/skills/idea-refine/SKILL.md`

Copy the skill instructions into this sandbox directory if running the experiment requires modifying or annotating them.

---

## What to record

During the session, note:
- Whether the "How Might We" reframe in Phase 1 changed how the problem was understood
- Which of the 5-8 variations in Phase 1 were genuinely surprising vs. obvious
- Whether the Phase 2 stress-test (user value / feasibility / differentiation) surfaced constraints that were implicit
- The "Not Doing" list verbatim — does it read like a scope section for a requirements doc?
- How many manual edits were needed to convert Phase 3 output to REQ-X-NNN format

These observations go into the lesson's "What We Learned" chapter.
