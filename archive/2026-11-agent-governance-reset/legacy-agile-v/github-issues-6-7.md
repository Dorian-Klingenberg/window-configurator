# Archived GitHub Issues: #6 and #7

This snapshot preserves the original issue framing that accompanied the
retired Agile V experiment. The issues remain on GitHub as closed history;
their product conclusions are summarized in the active
[product direction](../../../product-direction.md), and ongoing reconsideration
lives in GitHub issue #10.

## #6 — Write Pre-Phase A Draft Requirements (Level 1/2)

Original purpose: produce `pre-phase-a/draft-requirements.md` for the evolved
WindowConfigurator direction, covering the two entry flows, image and speech
inputs, measurement confidence, customer editing, physical remeasure, generic
webhooks, client-side pricing, and usability.

The issue required `REQ-SYS-NNN` and `REQ-SUB-NNN` identifiers and treated the
ConOps and Mission Objectives as prerequisites. It also identified #7 as its
dependent issue.

The requested artifact was completed in 2026. Its requirements-first method is
historical; its durable product conclusions were carried into the active
product direction document.

### Original issue body

> Produce pre-phase-a/draft-requirements.md covering Level 1/2 functional and
> non-functional requirements for the evolved WindowConfigurator product
> direction.
>
> Input: MVP scope and Not Doing list from Sandbox 002
> (sandboxes/002-idea-refine/OUTPUT.md)
> Format: Numbered requirement IDs (REQ-SYS-NNN for Level 1, REQ-SUB-NNN for
> Level 2)
>
> Must cover:
> - Two-entry-point workflow (contractor-led, customer-led)
> - Image processing pipeline (exterior photo to rough opening estimate)
> - Speech recognition pipeline (voice to structured configuration fields)
> - Measurement confidence model (customer-entered / image-estimate /
>   physical-confirmed)
> - Customer session (read-only sizing, editable aesthetics, live client-side
>   pricing)
> - Physical remeasure entry and order-ready gate
> - Outbound webhook events
> - NFRs: client-side pricing (no round-trips), no-expertise UX
>
> Blocked by: Pre-Phase A artifacts ConOps and Mission Objectives (both
> complete)
> Blocks: #7

## #7 — Slice D — evolved product direction (first Agile V slice)

Original purpose: create the first implementation slice under full Agile V.
It combined the expanded product direction with methodology requirements:
requirements before branching, Gherkin, TDD, an ATM, Tier 2 token and
verification metrics, and a dependency on #6.

This framing is retired. The product direction remains relevant, but the issue
did not identify a bounded implementation decision and its Agile V workflow is
no longer active. GitHub issue #10 replaces it as a flexible product-direction
tracker.

### Original issue body

> First implementation slice under full Agile V methodology. Scope to be
> defined once Pre-Phase A gate is satisfied.
>
> **This is no longer scoped to "demo UX hardening" only.** The Pre-Phase A
> work (2026-06-12) revealed a significantly expanded product direction:
> - Two-entry-point workflow (contractor-led on-site capture, customer-led
>   online)
> - Image processing pipeline for measurement estimation
> - Speech recognition pipeline for hands-free specification capture
> - Measurement confidence model gating pricing accuracy and order-readiness
> - Client-side pricing (preloaded grid, no round-trips)
> - Customer-appropriate UX (no window expertise required)
>
> **Slice D scope will be defined by the Draft Requirements artifact**, not by
> the prior "demo UX hardening" description.
>
> **Agile V requirements for this slice:**
> - Requirements doc written before branch opens
> - I also want to add gherkin as a required artifact to feed the coding agents
> - TDD throughout
> - ATM (Artifact Traceability Matrix) as closing artifact
> - Tier 2 metrics recorded: prompt count, test-to-req ratio, Red Team findings
>
> **Blocked by:** #6 (Draft Requirements)
