# Mission Objectives and Needs — WindowConfigurator

*Pre-Phase A artifact · Version 1.1 · 2026-06-12*

---

## 1. Mission Statement

WindowConfigurator enables window contractors to take a customer from first contact to signed order using a structured, measurement-confidence-driven workflow — with the customer actively involved in configuration choices at each stage where their input is meaningful.

---

## 2. Why This System Is Being Built

**Primary reason:** Portfolio demonstration of production-aligned agentic SDLC methodology. The system provides empirical evidence for an AI engineering contracting practice: that disciplined agentic development (Agile V, TDD, requirements-first, measurable quality) produces deployable, auditable software — not demos or prototypes.

**Secondary reason:** Real deployability. All portfolio projects are built to production standards. If a window contractor showed genuine interest, the system could be deployed for them. The production-alignment is not cosmetic.

---

## 3. Objectives

| # | Objective |
|---|---|
| O-01 | Support the full window replacement workflow from initial customer contact to manufacturer order placement. |
| O-02 | Enable two entry points into the workflow: contractor-initiated (on-site) and customer-initiated (online). |
| O-03 | Provide image processing-derived measurement estimates from exterior site photos, sufficient for indicative pricing and customer configuration. |
| O-04 | Track measurement confidence (customer-entered / image estimate / physical remeasure) as a first-class concept, with pricing accuracy and order-readiness gated accordingly. |
| O-05 | Allow customers to review and customize non-size configuration attributes after a contractor-generated session is issued. |
| O-06 | Lock sizing to the contractor once a contractor-provided measurement exists; prevent customer override of contractor measurements. |
| O-07 | Produce order-ready configurations only when a physical remeasure (confirmed measurement) is on file. |
| O-08 | Integrate with contractor CRM systems via outbound webhooks only — the platform never calls a CRM API. |
| O-09 | Demonstrate Agile V methodology applied to a real production codebase, with measurable quality and token cost data published as article evidence. |

---

## 4. Critical Success Criteria

| # | Criterion | Measurable signal |
|---|---|---|
| S-01 | Both entry points (contractor-led, customer-led) produce valid sessions that complete through to order-ready status. | End-to-end integration test passes for each flow. |
| S-02 | Measurement confidence tier is correctly reflected in pricing labels and order-readiness gates at all three levels. | Tests verify pricing label and order-gate behavior per source. |
| S-03 | Sizing is immutable by the customer after any contractor measurement is recorded. | Attempt to modify locked sizing returns an error. |
| S-04 | Image processing produces a rough opening estimate from a standard exterior photo within acceptable tolerance for estimate-grade pricing. | Defined tolerance TBD in Draft Requirements. |
| S-05 | All Pre-Phase A artifact types are produced before implementation begins on any slice covering the evolved workflow. | Artifact checklist complete; no slice branch opened until gate passes. |
| S-06 | Agile V adoption generates measurable Tier 2 data (prompt count, test-to-req ratio, Red Team findings) for at least one full slice. | `metrics/slice-d.md` populated at Slice D close. |
| S-07 | System is production-aligned: real window contractor could use it with no architectural changes. | No hardcoded tenant data, no dev-only shortcuts in production paths. |

---

## 5. Needs

These are the high-level system needs derived from the mission and objectives. They are precursors to Level 1/2 requirements and will be decomposed in the Draft Requirements artifact.

| # | Need |
|---|---|
| N-01 | The system shall support two session initiation modes: contractor-initiated and customer-initiated. |
| N-02 | The system shall accept exterior window photographs as input and derive estimated rough opening dimensions from them. |
| N-02b | The system shall accept voice input from the contractor and parse spoken window specifications into structured configuration fields (type, style, finish, grille pattern, etc.). |
| N-10 | The customer-facing configurator shall respond to configuration changes without server round-trips. The pricing grid shall be preloaded at session open; all pricing calculations shall execute client-side. |
| N-11 | The customer-facing configuration interface shall be operable by a homeowner with no window industry expertise. No jargon, no expert-required inputs. |
| N-12 | The production-adjacent implementation shall be Blazor-based. JavaScript shall be eliminated and replaced with C#. The existing Knockout.js + Bootstrap 3 frontend is the migration target. |
| N-03 | The system shall maintain a measurement source record for each dimension (customer-entered, image-estimate, physical-confirmed). |
| N-04 | The system shall compute and display pricing at all measurement confidence tiers, with labels appropriate to each tier. |
| N-05 | The system shall prevent order placement until a physical remeasure has been recorded for all items in the session. |
| N-06 | The system shall prevent customers from modifying sizing after a contractor measurement has been recorded. |
| N-07 | The system shall allow customers to modify all non-size configuration attributes (finish, hardware, grille, glass options) on a contractor-issued session. |
| N-08 | The system shall notify contractors of customer-initiated sessions and customer modifications via outbound webhook. |
| N-09 | The system shall produce a complete, manufacturer-ready order configuration upon contractor sign-off of a confirmed session. |

---

## 6. Strategic Context

WindowConfigurator is being developed as part of an AI engineering contracting portfolio. The codebase is the evidence base for an article series covering agentic SDLC methodology, token frugality, and Agile V adoption.

**Article series connection:**
- *S2-A02 (V-Model)* — WindowConfigurator is the live experiment ground
- *S3-A02 (Experimentation IS Requirements)* — Sandboxes 001/002 are the empirical evidence
- *S3-A03 (Derive the Product)* — Pre-Phase A artifact production is the case study

**Timeline constraint:** Pre-Phase A artifact production gates all implementation slices on the evolved workflow. No Slice D (or later) branch opens until the artifact checklist is complete.

**Token frugality:** Every Agile V component carries a measurable token cost. Each component must justify its cost against measurable quality improvement. This measurement discipline is itself a portfolio demonstration.
