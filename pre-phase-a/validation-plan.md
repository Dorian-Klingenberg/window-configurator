# Preliminary Validation Plan — WindowConfigurator

*Pre-Phase A artifact · Version 1.0 · 2026-06-12*

---

## Purpose

This document defines how the system will be validated against the user needs and success criteria established in `pre-phase-a/mission-objectives.md`. It establishes the measurement framework before implementation begins, so that evidence of satisfaction is collected systematically rather than asserted after the fact.

---

## Validation Strategy

Validation operates at two levels:

**Technical validation** — automated tests that verify each requirement is satisfied. Tests are written before implementation (TDD). Each test references at least one REQ-SYS-NNN or REQ-SUB-NNN from `pre-phase-a/draft-requirements.md`.

**Empirical validation** — metrics collected during and after each implementation slice that measure quality and process outcomes. These feed the Agile V adoption experiment and the article series.

---

## Success Criteria and Validation Targets

From `pre-phase-a/mission-objectives.md`:

| Criterion | Validation method |
|---|---|
| S-01: Both entry points produce valid sessions through to order-ready status | Integration test: contractor-led flow end-to-end; customer-led flow end-to-end |
| S-02: Measurement confidence tier correctly reflected in pricing labels and order gate | Unit tests: pricing label per source; order gate rejects non-PhysicalConfirmed |
| S-03: Sizing immutable after contractor measurement recorded | Unit test: PUT attempt on locked sizing returns error |
| S-04: Image processing produces rough opening estimate within acceptable tolerance | Field validation: compare image estimate vs. physical remeasure delta on ≥5 windows |
| S-05: All Pre-Phase A artifact types complete before implementation begins | Artifact checklist — this document is the final artifact |
| S-06: Agile V adoption generates measurable Tier 2 data for at least one full slice | `metrics/slice-d.md` populated at Slice D close |
| S-07: System is production-aligned — deployable with no architectural changes | No hardcoded tenant data; no dev-only shortcuts in production paths |

---

## Tier 2 Metrics — Collected Per Slice

Starting at Slice D (first slice past the Pre-Phase A gate), the following metrics are recorded in `metrics/slice-NNN.md`:

| Metric | What it measures | Baseline (Tier 1) | Paper benchmark |
|---|---|---|---|
| Tests added | Verification coverage growth | 7 (Phase 10 avg) | — |
| REQ-X-NNN written before branch opened | Requirements-first discipline | 0 (none existed pre-Agile V) | — |
| Requirements with passing test | Coverage completeness | N/A | 100% (H2) |
| Test-to-requirement ratio | Verification depth | N/A | 6.75:1 |
| Human prompts per cycle | Interaction cost | Not tracked | 6 (H3) |
| Red Team findings per cycle | Independent verification yield | Not tracked | 10 (Cycle 2) |
| ADRs written | Decision documentation density | 0–5 per phase | — |
| Fix commit ratio | Rework rate | ~5.5% (undercount likely) | — |
| Commits per slice | Complexity proxy | 1–4 (Phase 10 slices) | — |

---

## Requirement-Level Traceability

Every implementation slice that opens under Agile V must produce an ATM (Artifact Traceability Matrix) at close. Format: one row per requirement, three columns:

```
REQ-X-NNN | Implementation (file:class/method) | Test (file:method)
```

The ATM is the evidence that H2 (100% requirement-level verification) is met. It is a required closing artifact — no slice merges to main without it.

---

## Red Team Protocol

Test design for each slice must follow the Red Team Protocol documented in `AGENTS.md` (Testing Conventions section):

- The agent or prompt session designing tests receives only the requirements file — never the implementation.
- Build Agent (code) and Test Designer (tests) work from the same requirements source with no shared context.
- This structural separation is what makes Red Team findings meaningful. A test suite written with visibility into the implementation cannot falsify that implementation.

---

## Pre-Phase A Validation Gate

This document, combined with the other four Pre-Phase A artifacts, satisfies the gate condition for opening the first Agile V implementation slice.

| Artifact | File | Complete |
|---|---|---|
| ConOps | `pre-phase-a/conops.md` | ✅ |
| Mission Objectives & Needs | `pre-phase-a/mission-objectives.md` | ✅ |
| System Architecture Diagrams | `pre-phase-a/architecture-diagrams.md` | ✅ |
| Draft Requirements (Level 1/2) | `pre-phase-a/draft-requirements.md` | ✅ |
| Preliminary Validation Plan | `pre-phase-a/validation-plan.md` | ✅ |

**Pre-Phase A gate: SATISFIED. Implementation slices may now open.**

---

## Field Validation — Image Processing Tolerance

S-04 requires field validation that cannot be done through automated tests alone. Before the image processing pipeline is declared production-ready, a real-world test must be performed:

1. Photograph ≥5 windows from the exterior using the contractor mobile portal.
2. Record the image-processing estimate for each.
3. Perform physical remeasure (interior frame) on each.
4. Calculate delta between estimate and physical measurement.
5. Define acceptable tolerance (to be specified in the image processing ADR).

Until this field validation is performed and a tolerance is documented, the image processing pipeline is estimate-grade only and must be labeled as such in all customer-facing surfaces.

---

## Token Cost Tracking

Token cost is a first-class measurement target for the Agile V adoption experiment. Before each session, record intent and estimated scope. After each session, record estimated token load. Pattern emerges over time: which Agile V components pay for themselves, which do not.

This data is article content. It is the empirical evidence for the token frugality methodology.

Model tiering guidance (to be refined from measurement data):
- Formulaic tasks (ADR formatting, test scaffolding from pattern, doc cross-referencing): candidates for smaller model once pattern is confirmed.
- Architectural reasoning, requirements analysis, Red Team verification: full model required.
