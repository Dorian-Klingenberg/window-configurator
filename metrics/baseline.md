# Tier 1 Baseline — Phases 0–10C (Pre-Agile V)

*Extracted from git history on 2026-06-11, before Slice D begins.*

This file is the pre-intervention baseline. Slice D and forward produce Tier 2 data. After each slice, compare Tier 2 against these numbers to answer: **did the quality improvement justify the token cost?**

---

## Observation: commit discipline was coarse before Phase 10

Before Phase 10, multiple phases were batched into single commits with informal messages ("Phase 8 complete.", "Completed up to step 9.5.", "Work done on the 18th and not commit."). This makes per-phase reconstruction approximate for Phases 2–9. Phase 10 introduced slice-level branching, which produced finer-grained history.

This is itself a Tier 1 data point: **coarse commit granularity = low traceability**. Agile V adds that.

---

## Test Count

**Total [Fact] / [Theory] methods:** 162  
**Test files:** 31 (excluding obj/ artifacts)

### Distribution by concern (best-effort phase mapping)

| Concern | Phase(s) | Test files | Test count |
|---|---|---|---|
| Domain entities | 1 | 3 | 18 |
| Repository + DbContext | 2 | 4 | 25 |
| Catalog service | 3 | 1 | 7 |
| Pricing service | 4 | 6 | 46 |
| Completion validation | 5 | 1 | 2 |
| API surface + controllers | 6 | 4 | 37 |
| Webhook dispatch | 7 | 2 | 4 |
| API key / retry / security | 8 | 6 | 15 |
| Session bootstrap client | 9 | 1 | 1 |
| Phase 10 demo integration | 10 | 3 | 7 |
| **Total** | | **31** | **162** |

**Cumulative growth (reconstructed):**

| After phase | Cumulative tests | Added |
|---|---|---|
| Phase 1 | 18 | 18 |
| Phase 2 | 43 | 25 |
| Phase 3 | 50 | 7 |
| Phase 4 | 96 | 46 |
| Phase 5 | 98 | 2 |
| Phase 6 | 135 | 37 |
| Phase 7 | 139 | 4 |
| Phase 8 | 154 | 15 |
| Phase 9 | 155 | 1 |
| Phase 10 (A+B+C) | 162 | 7 |

*Note: phase mapping is best-effort based on test subject matter. No per-phase commit checkout was performed.*

---

## ADR Count

**Total ADRs:** 18

### Distribution by phase

| Phase(s) | ADRs | Titles |
|---|---|---|
| 1 | 0001–0005 | Unidirectional webhook model, two-flow architecture, session-centric domain, product-line as session input, completion payload contract |
| 4–5 | 0006 | Runtime pricing grid alignment and completion error parity |
| 6 | 0007 | Minimal versioned session API surface |
| 7 | 0008–0009 | Webhook dispatch on session submit, durable webhook delivery tracking |
| 8 | 0010–0014 | Webhook retry, tenant integration settings, API key scope, E2E test harness, background retry orchestrator |
| 10 | 0015–0018 | Mock demo host architecture, iframe embedding, postMessage signaling, CRM polling and session API extension |

**ADR density observation:** 5 ADRs in Phase 1 (architecture-heavy, decision-rich). 1–2 per phase thereafter. Phase 9 produced zero ADRs despite introducing significant planning work. ADR writing was reactive, not systematic.

---

## Fix Commit Ratio

**Method:** commits whose primary purpose was correcting a defect, not advancing a feature.

| Commit | Phase | Description |
|---|---|---|
| b1fb967 | 4–5 | "More fixes to getting the pricing corrected" |
| 971b71e | 10A | "fix: remove webRoot from openExternally serverReadyAction" |

**Clearly labeled fix commits: 2**

Additional commits contain incidental fixes embedded in feature work (e.g., "Phase 10 Slice A: ... VS Code debug reliability fixes"). These are not counted as fix commits because the primary purpose was feature delivery.

**Reconstructed fix commit ratio:** 2 / ~36 implementation commits ≈ **5.5%**

*Caveat: pre-Phase 10 commit discipline was coarse. Rework may be underrepresented in fix commit count — early commits batched multiple sessions and likely included untracked rework.*

---

## Commit Count Per Slice

| Phase / Slice | Commits (main) | Notes |
|---|---|---|
| Phase 0 (setup) | 3 | gitignore, project files, initial housekeeping |
| Phase 1 | 3 | Core domain + ADRs + test scaffold |
| Phases 2–5 | ~3 | Batched — cannot separate with confidence |
| Phase 6 | 1 | Single batch commit |
| Phase 7 | 1 | Single batch commit |
| Phase 8 | 1 | Single batch commit |
| Phase 9 | 6 | Mostly planning/transition, not implementation |
| Phase 10 Slice A | 4 | Feature + merge commits |
| Phase 10 Slice B | 3 | Feature + merge commits |
| Phase 10 Slice C | 1 | Single squash commit |
| Docs / housekeeping | ~13 | ADR additions, bootstrap files, Agile V setup |
| **Total (main)** | **~39** | |

*Phases 2–8 are systematically underrepresented due to batched commits. Phase 10 slices are the first period where branching and per-slice commit discipline is visible.*

---

## Documentation Artifact Count

| Artifact type | Count | Notes |
|---|---|---|
| ADRs | 18 | `adr/0001–0018.md` |
| Journal entries | 12 | `journal/YYYY-MM-DD.md` — first entry: 2026-05-11 |
| Lessons | 1 | `lessons/2026-06-11_agile-v-experiment-design.md` — written at Agile V adoption |
| Implementation roadmap | 1 | `implementation-roadmap.md` |
| Domain knowledge | 1 | `window-domain-knowledge.md` |
| **Total substantive** | **33** | |

**Documentation density:** 33 artifacts across ~10 implementation phases = ~3.3 per phase average. ADRs dominate (18 of 33). Journals cover only 12 session dates across the full project history — many sessions were not journaled.

---

## Summary for Slice D Comparison

When Slice D closes, compare Tier 2 data against these benchmarks:

| Metric | Baseline value | Tier 2 measurement |
|---|---|---|
| Tests added per slice | 7 (Phase 10 avg) · 18–46 (earlier phases) | Count [Fact]/[Theory] added in Slice D |
| ADRs per slice | 0–5 (wide range) | Count ADRs written during Slice D |
| Fix commit ratio | ~5.5% (undercount likely) | Fix commits / total Slice D commits |
| Commits per slice | 1–4 (Phase 10 slices) | Commit count on Slice D branch |
| Requirements written before coding | 0 (none existed) | Count REQ-D-NNN written before branch opened |
| Requirements with passing test | N/A | REQ-D-NNN covered by at least one test |
| Test-to-requirement ratio | N/A (no requirements existed) | Tests added ÷ REQ-D-NNN count (paper benchmark: 6.75:1) |
| Human prompts per cycle | Not tracked | Count prompts per Infinity Loop cycle (paper benchmark: 6) |
| Red Team findings per cycle | Not tracked | Count findings per test-design review pass (paper benchmark: 10 in Cycle 2) |

The primary Agile V question for Slice D: **does requirements-first + TDD produce a measurable improvement in the fix commit ratio and test coverage, and what is the token cost of that improvement?**
