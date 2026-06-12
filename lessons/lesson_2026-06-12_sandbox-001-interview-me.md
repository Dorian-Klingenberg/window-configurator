# Lesson: interview-me Sits Above Requirements

*2026-06-12 — Sandbox 001: where interview-me fits in the Pre-Phase A hierarchy*

---

## Chapter 1: What We Were Testing

The hypothesis for Sandbox 001 was that the `interview-me` skill's output structure (Outcome / User / Why now / Success / Constraint / Out of scope) would map closely enough to REQ-X-NNN format that the translation step would be mechanical. If true, the skill could sit directly at the requirements gate and compress Pre-Phase A.

The test case was Slice D "demo UX hardening" — except the session revealed that the actual scope in Dorian's head was considerably larger than Slice D. That divergence is itself the most important finding.

---

## Chapter 2: How the Interview Actually Ran

The skill ran in two phases. Before it was formally invoked, Dorian described two complete user flows — contractor-led and customer-led — including a measurement confidence model that invalidated the current domain assumption (rough opening as binary: null or populated). This was the ConOps, produced organically without a structured prompt.

When the skill was formally invoked, it took five questions to reach the stop condition. The interview confirmed what the organic phase surfaced, expanded it in one key place (production-aligned, not just portfolio), and established the binding constraints (no feasibility studies, no implementation during Pre-Phase A, SE/Agile V hybrid structure).

The stop condition — "can I predict the user's reaction to the next three questions?" — was testable and clean. It is a reliable signal.

---

## Chapter 3: Where interview-me Actually Fits

The hypothesis was wrong in a specific, useful way.

The output does NOT map to REQ-X-NNN. It maps to the inputs for Pre-Phase A artifacts. The hierarchy is:

```
interview-me → ConOps + Mission Objectives → Draft Requirements (L1/L2) → REQ-X-NNN
```

The skill extracts confirmed intent. That intent seeds the ConOps (operational scenarios) and Mission Objectives (why / success criteria). Those documents then enable Level 1/2 requirements to be written. Implementation-level REQ-X-NNN statements come after that.

Placing interview-me directly at the requirements gate skips two steps. It belongs one level higher — at the Pre-Phase A entry gate, before any artifact drafting begins.

---

## Chapter 4: The Scope Expansion Signal

The organic braindump that preceded the formal interview is worth examining. Dorian had not been asked about image processing or the two-entry-point workflow. They emerged because the conversation opened space for them.

This is the skill working as designed. The interview-me documentation says to watch for "should want" signals — answers that deflect to convention rather than expressing actual intent. The equivalent here was the opposite: once the structured interview created a receptive context, scope that had been implicit became explicit.

The implication: run interview-me before assuming you know what the next slice is. The scope in the stakeholder's head and the scope implied by the roadmap are not always the same thing. In this session, they were significantly different.

---

## Chapter 5: What We Learned

- interview-me belongs at the Pre-Phase A entry gate, not the requirements gate. Its output is confirmed intent, not requirements.
- The confirmed statement of intent (Outcome / User / Why now / Success / Constraint / Out of scope) maps cleanly to ConOps and Mission Objectives inputs — those two artifact types are the natural next step after a confirmed interview.
- The stop condition ("predict the next three questions") is a reliable, testable signal — not a vibe.
- Organic scope emergence is real. Leave space before the structured interview for a braindump. What comes out unsolicited is often more valuable than what the questions surface.
- Running the skill against "the next slice" may reveal that the next slice is not the right unit of work. Be prepared to zoom out.

---

## What Comes Next

1. Sandbox 002 — `idea-refine`: take the confirmed intent and the two flows through divergent/convergent processing. The expected output is a Phase 3 one-pager with MVP scope and "Not Doing" list that seeds the Draft Requirements artifact.
2. Pre-Phase A artifact production (using interview-me output as seed):
   - ConOps: formalize the two flows from this session
   - Mission Objectives & Needs: formalize the confirmed statement of intent
   - System Architecture Diagrams: build on ADRs 0001–0018, extend for evolved system
   - Draft Requirements (Level 1/2): from Sandbox 002 output
   - Preliminary Validation Plan: formalize metrics framework from `metrics/baseline.md`
3. No implementation until all five artifact types are produced.
