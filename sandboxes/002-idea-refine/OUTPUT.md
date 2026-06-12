# Sandbox 002 — idea-refine: Output

*Session: 2026-06-12*

---

## Input

Confirmed intent from Sandbox 001 (interview-me): a two-sided window replacement platform with two entry points, image processing + speech recognition for on-site capture, and a measurement confidence model gating pricing accuracy and order-readiness.

---

## Phase 1 — Divergent

"How Might We" reframe: *How might we make the entire window replacement process — from standing outside a house to signed order — feel like something a contractor can do with one hand, while still giving the customer enough control to feel ownership over the result?*

Clarifying input: average job is 5 windows. Overarching goal is to save the contractor time specifying and discussing options — time they could spend on billable installation work.

Eight variations generated. Six resonated:

1. Minimum contractor touchpoints — photo + voice, confirm, send link, done
2. Contractor as measurement expert only — never discusses aesthetics
3. Batch capture for multi-window jobs — one walk, all windows, one review screen
4. Customer as first photographer — optional, removes first visit for simple jobs
5. ~~Image estimate sufficient to skip remeasure~~ — **Rejected: 3/32" accuracy requires physical interior remeasure. Non-negotiable.**
6. Zero-friction customer delivery — text link, no login, no account
7. ~~Pipeline dashboard~~ — **Rejected: handled by contractor's CRM. Out of system boundary.**
8. Contractor app as only on-site tool needed

---

## Phase 2 — Convergent

Six resonant variations clustered into three directions:

**Direction A — Compress contractor on-site time** (1, 3, 8)
One mobile app, batch capture, customer link sent before leaving the driveway.

**Direction B — Remove contractor from options conversation** (2, 6)
Contractor touches measurements and window type only. Customer self-serves all aesthetics via zero-friction link.

**Direction C — Customer compresses first visit** (4)
Customer photographs own windows; contractor reviews remotely. Feasibility risk: unpredictable customer photo quality.

Stress-test result:
- A and B are complementary, not competing. A without B still leaves the contractor in options calls. B without A still has a slow on-site process.
- C is a Phase 2 feature — the assumption (customer takes a usable photo) is fragile and not worth betting the MVP on.

**Recommended direction: A + B as MVP core. C as named future capability.**

---

## Phase 3 — One-Pager

### Problem Statement

Window contractors lose billable installation time to a specification-and-discussion process that is largely automatable. On-site measurement and specification is manual and slow. Existing manufacturer and retail configurator tools (Home Depot, manufacturer portals) compound the problem: they are expert-only and server-round-trip-slow, making the customer options conversation a burden rather than a self-service handoff. Every hour spent specifying and discussing is an hour not spent installing.

### Recommended Direction

Build a two-sided platform where the contractor's on-site job is capture-only (photo + voice, hands-free, one mobile app), and the customer's job is self-service options selection (zero-friction link, no login, live pricing that responds instantly without server round-trips). The contractor and customer never need to discuss aesthetics. The contractor re-enters only to confirm the final remeasure and sign the order.

### Key Assumptions and Validation

| Assumption | Validation approach |
|---|---|
| Contractors will trust image-derived estimates enough to send a customer link before physical remeasure | Field test: compare image estimate vs. remeasure delta on 5 windows with a real contractor |
| Customers will engage with the link without contractor hand-holding | Measure link open rate and completion rate in pilot |
| Voice parsing reliably extracts window specification from natural contractor speech | Run 50 spoken specifications through the pipeline; measure parse accuracy |
| Average 5-window job captured on-site in under 10 minutes | Time a contractor through a batch capture session |
| Preloaded pricing grid produces accurate client-side calculations | Validate client-side results against server-side calculation on submission |

### MVP Scope

- Contractor mobile portal: photo + voice per window, batch review and correction, session creation, customer link generation (text/link, no app required for customer)
- Image processing pipeline: exterior photo → estimated rough opening
- Speech recognition pipeline: spoken specification → structured configuration fields
- Customer session: read-only sizing, editable aesthetics (finish, hardware, grille, glass), live pricing with confidence label — all client-side, no per-change server round-trip
- Measurement confidence model: customer-entered / image-estimate / physical-confirmed with appropriate pricing labels
- Physical remeasure entry: contractor records interior frame + brick mold dimensions, unlocks order-ready status
- Outbound webhook on key events: session created, customer modifications submitted, order ready

### Not Doing (and Why)

| Not doing | Why |
|---|---|
| Image processing for final measurements | 3/32" accuracy requires physical interior remeasure after pulling casing — image processing cannot achieve this |
| Pipeline / job management dashboard | Contractor's CRM handles this. We fire webhooks; job tracking is out of our boundary |
| Customer photography for initial estimate | Customer photo quality unpredictable; poor estimate damages pricing trust. Phase 2 at earliest |
| Server round-trip per configuration change | Existing tools are already slow and it kills engagement. Pricing grid preloads at session open; calculations are client-side |
| Login / accounts for customers | Friction kills engagement. Session token in a link is sufficient |
| Expert-required configuration UI | Existing tools already occupy this space. Our differentiation is the opposite |
| Payment processing | Outside system boundary |
| Manufacturer communication | Outside system boundary |
| Contractor options discussion support | The system replaces this conversation entirely |

---

## What This Tells the Project

The idea-refine skill produced the MVP scope and "Not Doing" list from the confirmed intent in approximately one structured session. The one-pager maps directly to Draft Requirements inputs — the MVP scope items translate to Level 1 functional requirements, and the "Not Doing" list becomes the scope exclusion section.

**Verdict:** idea-refine earns a slot between interview-me and Draft Requirements in the Pre-Phase A sequence. The two-skill sequence is: interview-me → confirmed intent → idea-refine → MVP scope + Not Doing list → Draft Requirements (Level 1/2).
