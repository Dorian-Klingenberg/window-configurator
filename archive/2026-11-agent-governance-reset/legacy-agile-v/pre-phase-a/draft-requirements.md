# Draft Requirements — WindowConfigurator

*Pre-Phase A artifact · Version 1.0 · 2026-06-12*
*Level 1 (system) and Level 2 (subsystem) requirements*

---

## Traceability

| Requirement | Source |
|---|---|
| REQ-SYS-001 through REQ-SYS-013 | MVP scope — `sandboxes/002-idea-refine/OUTPUT.md` |
| REQ-SYS-014 through REQ-SYS-016 | NFRs — `pre-phase-a/mission-objectives.md` N-10, N-11, N-12 |
| REQ-SUB-* | Derived from parent REQ-SYS-* |
| Scope exclusions | "Not Doing" list — `sandboxes/002-idea-refine/OUTPUT.md` |

---

## Level 1 — System Requirements

### Functional

**REQ-SYS-001 — Contractor-initiated session**
The system shall support contractor-initiated session creation from on-site capture inputs (exterior photograph and spoken window specification).

**REQ-SYS-002 — Customer-initiated session**
The system shall support customer-initiated session creation from online self-configuration without contractor involvement.

**REQ-SYS-003 — Image processing pipeline**
The system shall accept exterior window photographs as input and derive estimated rough opening dimensions from them.

**REQ-SYS-004 — Speech recognition pipeline**
The system shall accept spoken contractor input and parse window specifications into structured configuration fields.

**REQ-SYS-005 — Measurement source tracking**
The system shall maintain a measurement source attribute for every sizing dimension in a session item. Valid sources are: CustomerEntered, ImageEstimate, PhysicalConfirmed.

**REQ-SYS-006 — Confidence-tiered pricing**
The system shall compute and display pricing at all measurement confidence tiers, with a pricing label appropriate to the measurement source of the session item.

**REQ-SYS-007 — Physical remeasure gate**
The system shall prevent order placement until every item in the session carries a PhysicalConfirmed measurement source.

**REQ-SYS-008 — Contractor measurement lock**
The system shall prevent customers from modifying sizing fields after any contractor measurement (ImageEstimate or PhysicalConfirmed) has been recorded for that item.

**REQ-SYS-009 — Customer attribute editing**
The system shall allow customers to modify non-size configuration attributes (finish, hardware, grille pattern, glass options) on contractor-issued sessions, regardless of measurement source.

**REQ-SYS-010 — Zero-friction customer link**
The system shall generate a shareable customer session link that requires no login, no account creation, and no application installation to access.

**REQ-SYS-011 — Outbound webhook events**
The system shall emit outbound webhook events to the configured tenant endpoint on the following state transitions: session created, customer modifications submitted, session order-ready.

**REQ-SYS-012 — Physical remeasure entry**
The system shall accept physical remeasure inputs from the contractor and update the measurement source of affected items to PhysicalConfirmed.

**REQ-SYS-013 — Order-ready configuration**
The system shall produce a complete, manufacturer-ready order configuration upon contractor sign-off of a session where all items are PhysicalConfirmed.

### Non-Functional

**REQ-SYS-014 — Client-side pricing performance**
The customer-facing configurator shall respond to configuration changes without server round-trips. The pricing grid shall be preloaded at session open. All pricing calculations shall execute client-side.

**REQ-SYS-015 — Customer usability**
The customer-facing configuration interface shall be operable by a homeowner with no window industry expertise. No jargon, no expert-required input fields, no unexplained option sets.

**REQ-SYS-016 — Blazor / C# implementation**
The production-adjacent implementation shall be Blazor-based. JavaScript shall be eliminated and replaced with C#. The existing Knockout.js + Bootstrap 3 frontend is the migration target.

---

## Level 2 — Subsystem Requirements

### Session initiation

**REQ-SUB-001** *(from REQ-SYS-001)*
Contractor-initiated sessions shall be created from at minimum two inputs: one exterior photograph per window opening and one spoken specification per window opening. Both inputs are required to create a session item.

**REQ-SUB-002** *(from REQ-SYS-001)*
The contractor portal shall support batch capture: multiple windows photographed and specified in a single on-site workflow, reviewed together before session creation.

**REQ-SUB-003** *(from REQ-SYS-002)*
Customer-initiated sessions shall accept customer-entered sizing (width, height) as optional inputs. If omitted, sizing fields remain blank and pricing is suppressed until sizing is provided.

### Image processing

**REQ-SUB-004** *(from REQ-SYS-003)*
The image processing pipeline shall produce a rough opening estimate expressed in the system's fractional measurement format (sign, whole, numerator, denominator). It shall not produce floating-point output.

**REQ-SUB-005** *(from REQ-SYS-003)*
The image processing pipeline shall label its output as ImageEstimate and shall not represent its output as suitable for order placement.

### Speech recognition

**REQ-SUB-006** *(from REQ-SYS-004)*
The speech recognition pipeline shall parse at minimum: window type (e.g., casement, double-hung, awning, picture), operation (e.g., left-hand, right-hand), grille pattern, and exterior finish.

**REQ-SUB-007** *(from REQ-SYS-004)*
Parsed speech output shall be presented to the contractor for review and correction before session creation. The contractor confirms or edits; the system does not auto-commit voice input.

### Measurement confidence

**REQ-SUB-008** *(from REQ-SYS-005, REQ-SYS-006)*
Pricing labels per measurement source:
- CustomerEntered: "Indicative estimate — subject to survey"
- ImageEstimate: "Estimate — based on site photo"
- PhysicalConfirmed: "Confirmed price"

**REQ-SUB-009** *(from REQ-SYS-005)*
When a higher-confidence measurement source is recorded for an item, it supersedes the prior source. Source transitions are one-directional: CustomerEntered → ImageEstimate → PhysicalConfirmed. Downgrade is not permitted.

### Physical remeasure

**REQ-SUB-010** *(from REQ-SYS-012)*
Physical remeasure inputs shall include: interior frame width, interior frame height, exterior brick mold width, exterior brick mold height. All four values are required to transition an item to PhysicalConfirmed.

**REQ-SUB-011** *(from REQ-SYS-012)*
Physical remeasure inputs shall use the system's fractional measurement format. Floating-point input is not accepted.

### Customer session

**REQ-SUB-012** *(from REQ-SYS-010)*
The customer session link shall be a single URL containing a signed session token. Token expiry and revocation are controlled by the contractor or tenant. No password, no account.

**REQ-SUB-013** *(from REQ-SYS-009)*
Sizing fields shall be rendered as read-only in the customer session when the measurement source is ImageEstimate or PhysicalConfirmed. The read-only state shall be visually distinct and accompanied by a label indicating the source.

### Pricing

**REQ-SUB-014** *(from REQ-SYS-014)*
The pricing grid (all product lines, option pricing deltas, base prices) shall be serialized and transmitted to the client at session open in a single response. Subsequent pricing calculations shall not require a network call.

**REQ-SUB-015** *(from REQ-SYS-014)*
Client-side pricing calculations shall produce results identical to server-side calculations for the same inputs. Server validates the submitted price on completion; client-side result is for display only.

### Webhooks

**REQ-SUB-016** *(from REQ-SYS-011)*
Webhook payloads shall not include CRM-specific fields. The payload contract is generic; CRM mapping is the CRM's responsibility.

**REQ-SUB-017** *(from REQ-SYS-011)*
Webhook delivery shall include retry behavior on failure, consistent with the existing webhook delivery infrastructure (ADR 0009, ADR 0010).

---

## Scope Exclusions

The following are explicitly out of scope. These are architectural decisions, not deferred features.

| Exclusion | Rationale |
|---|---|
| Image processing for final (order-grade) measurements | 3/32" accuracy requires physical interior remeasure. Image processing cannot achieve this. The physical remeasure is mandatory for every job. |
| Server round-trip per configuration change | Performance constraint (REQ-SYS-014). Existing tools fail here; this system must not. |
| Customer modification of contractor-provided sizing | Measurement authority belongs to the contractor. Customer cannot override (REQ-SYS-008). |
| Pipeline / job management dashboard | Contractor's CRM handles job tracking. System fires webhooks; pipeline visibility is out of boundary. |
| Customer photography for initial estimate | Customer photo quality is unpredictable; poor estimate damages pricing trust. Candidate for a future phase after field validation. |
| Login or account creation for customers | Friction kills engagement. Session token in a link is sufficient (REQ-SYS-010). |
| Payment processing | Outside system boundary. |
| Manufacturer communication | Outside system boundary. Contractor places orders through their own channel. |
| JavaScript in production-adjacent implementation | REQ-SYS-016. Blazor / C# replaces all JavaScript. |
| CRM API calls | Non-negotiable architectural constraint (ADR 0001). System fires outbound webhooks only. |
