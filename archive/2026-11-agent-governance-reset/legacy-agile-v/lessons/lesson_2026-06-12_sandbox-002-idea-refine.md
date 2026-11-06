# Lesson: idea-refine Bridges Intent to Draft Requirements

*2026-06-12 — Sandbox 002: where idea-refine fits and what the "Not Doing" list actually does*

---

## Chapter 1: What We Were Testing

Sandbox 002 asked whether idea-refine's Phase 3 one-pager — specifically its MVP scope and "Not Doing" list — could serve as the direct input to Draft Requirements, bridging the gap between the confirmed intent from Sandbox 001 and Level 1/2 requirement statements.

The test subject was the full WindowConfigurator product direction as clarified through the session: two-entry-point workflow, image processing + speech recognition for on-site capture, measurement confidence model, and the overarching goal of saving contractor time.

---

## Chapter 2: How the Session Ran

Phase 1 (Divergent) generated eight variations using inversion, simplification, amplification, and constraint removal. Two were immediately rejected by the user on hard technical or boundary grounds:

- Image processing for final measurements: rejected because 3/32" accuracy requires physical interior remeasure. This is a non-negotiable technical constraint.
- Pipeline dashboard: rejected because job management is the CRM's responsibility. This is a boundary constraint, not a capability gap.

Both rejections produced immediate "Not Doing" entries — which is exactly the skill working as designed. The divergent phase surfaced the constraints by proposing things that violated them.

Phase 2 (Convergent) clustered six resonant variations into three directions. The key finding was that two directions (contractor on-site efficiency and customer self-service) are not competing alternatives — they are complementary halves of the same product. A customer who can't self-serve in a fast, simple configurator puts the options conversation back on the contractor's plate. A contractor who captures on-site quickly but still has to explain grille patterns on the phone has saved nothing.

A critical addition arrived between phases: performance and usability as non-functional requirements. Existing window configuration tools (manufacturer portals, Home Depot) are slow — every configuration change triggers a server round-trip to overloaded infrastructure — and expert-only. These are the two reasons the options conversation stays with the contractor instead of moving to the customer. The "Not Doing" list explicitly excludes the server-round-trip pattern. The MVP scope includes client-side pricing calculation from a preloaded grid.

---

## Chapter 3: What the "Not Doing" List Actually Does

The "Not Doing" list is not a backlog. It is a scope contract.

Each entry explains why a capability is excluded — not just that it is excluded. That explanation does three things:

1. It tells the implementation agent which direction to avoid when ambiguity arises. "No server round-trip per configuration change" is an architectural decision, not just a feature deferral.
2. It tells the next requirements author which requirement IDs to not write. "Pipeline dashboard is out of boundary" means no REQ-X-NNN will ever be written for job management views.
3. It prevents the contractor options discussion from reappearing in a different form. "The system replaces this conversation" is a design principle, not a cut feature.

The "Not Doing" list is arguably the most durable artifact the skill produces. The MVP scope list changes as slices ship. The "Not Doing" list changes only when a boundary decision is explicitly revisited.

---

## Chapter 4: The Two-Skill Sequence

The relationship between Sandboxes 001 and 002 is now clear:

```
interview-me
    → confirmed intent (Outcome / User / Why now / Success / Constraint / Out of scope)
        → idea-refine
            → MVP scope + "Not Doing" list
                → Draft Requirements (Level 1/2)
                    → slice REQ-X-NNN
```

Neither skill alone gets to requirements. Together they produce the inputs that make requirements writing mechanical rather than creative. The interview extracts *what and why*. The refinement produces *what exactly and what never*.

---

## Chapter 5: What We Learned

- idea-refine earns a slot between interview-me and Draft Requirements. The two-skill sequence is the Pre-Phase A entry process.
- The divergent phase surfaces hard constraints by proposing things that violate them. The rejections are as valuable as the acceptances.
- Performance and usability are non-functional requirements that belong in the one-pager, not as an afterthought in implementation. Slow + expert-required is the competitive baseline. Fast + no-expertise-required is the differentiation.
- The "Not Doing" list is a scope contract and an architectural decision record, not a deferred backlog. Write it with reasons.
- Direction clustering (Phase 2) revealed that what appeared to be competing directions were actually complementary. Neither A nor B works without the other.

---

## What Comes Next

1. Update `pre-phase-a/conops.md` and `pre-phase-a/mission-objectives.md` with NFRs (performance, usability) and the validated MVP scope.
2. Produce Draft Requirements (Level 1/2) from the MVP scope — the next Pre-Phase A artifact.
3. Produce System Architecture Diagrams extending ADRs 0001–0018 for the evolved system.
4. Produce Preliminary Validation Plan formalizing the metrics framework.
5. Open Slice D branch once all five Pre-Phase A artifact types are complete.
