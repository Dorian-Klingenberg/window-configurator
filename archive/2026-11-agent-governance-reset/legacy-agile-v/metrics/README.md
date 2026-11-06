# Metrics

This directory tracks the Agile V adoption experiment. The primary question: does each Agile V component produce measurable quality improvement relative to its token cost?

---

## Measurement tiers

### Tier 1 — Reconstructed from git history (Phases 0–9 baseline)

Extractable today. These are the pre-intervention numbers.

| Metric | What it measures |
|---|---|
| Test count per phase | Growth of verification coverage |
| ADR count per phase | Architecture decision density |
| Fix commit ratio | Rework rate — commits that fix rather than advance |
| Commit count per slice | Complexity proxy |
| Documentation artifact count | Docs density relative to code volume |

**File:** `metrics/baseline.md` ← to be populated before Slice D begins

### Tier 2 — Measured from Slice D forward

Requires deliberate instrumentation. Recorded per slice as work completes.

| Metric | What it measures |
|---|---|
| Requirement count per slice | Scope formalisation level |
| Requirement coverage % | Fraction of requirements with a passing test |
| Estimated session token load | Token cost of each Agile V component |
| Agile V step adherence | Did the slice follow the prescribed order? |

**Files:** `metrics/slice-d.md`, `metrics/slice-e.md`, etc.

### Tier 3 — Future

Scope drift rate, Red Team Verifier finding rate, cost-to-quality ratio. Not measured until tooling and enough Tier 2 data exist to make them meaningful.

---

## Decision rule

After each slice ships, review Tier 2 data against the Tier 1 baseline. The question is not "did quality go up?" in isolation — it is "did quality go up enough to justify the token cost?" If the answer is yes, add the next Agile V component to the following slice. If no, hold or adjust before expanding.

---

## Files

| File | Contents | Status |
|---|---|---|
| `baseline.md` | Tier 1 numbers from git history (Phases 0–9) | ⬜ Not yet written |
