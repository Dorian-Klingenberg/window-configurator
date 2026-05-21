# Window Industry Domain Knowledge

This file captures domain knowledge about the residential window industry as it applies to the WindowConfigurator platform. It is intended as a reference for AI-assisted development work on this codebase.

---

## The Buying Journey

There are two distinct customer types with different levels of technical knowledge.

### Contractors (B2B)

A contractor is a professional installer or renovation company. They understand all three measurement types, they know the difference between product lines, and they are usually working on behalf of a homeowner client. They interact with this platform through a CRM-initiated flow — their CRM (Salesforce, HubSpot, etc.) launches a configurator session pre-loaded with job context, they configure the window, and the result flows back to the CRM.

Contractors are expected to understand:
- All three measurement types (frame, rough opening, outside)
- Product lines and their differences
- Operational styles
- Brickmould presence and its effect on measurements
- The difference between a quote and a purchase order

### Prospects / Homeowners (B2C)

A prospect is a homeowner visiting a dealer's website. They are not expected to know industry terminology. They click "Build My Window," enter their email, receive a magic link, and configure their window at a high level. The result is sent to the dealer as a lead. The prospect's quote is informational — a contractor will always remeasure before an order is actually placed.

---

## The Three Critical Measurements

Every window in a job requires three distinct measurements. These are not interchangeable and are used at different stages of the process.

### 1. Frame Measurement

- **Definition:** Width and height from outside edge to outside edge of the inner portion of the window frame, including the jamb extension.
- **What it is:** The window unit size. This is what you order from the manufacturer.
- **Who provides it:** At quote time, the contractor or prospect inputs this.
- **Technical note:** The jamb extension (also called jamb depth) is part of the frame — it extends the frame to fit the wall thickness.

### 2. Rough Opening Measurement

- **Definition:** The actual structural opening in the wall — the hole the window sits in.
- **Rule of thumb:** Approximately frame + ½" on each side and top and bottom (frame width + 1", frame height + 1"). This is a planning approximation only.
- **Critical point:** The rough opening must be physically measured by the contractor on-site during a **remeasure visit** before the order is placed. The casing (interior trim) is pulled off, and the actual opening is measured. The real rough opening can differ from the rule-of-thumb estimate.
- **Workflow implication:** Rough opening fields are **null at quote time** and populated by the contractor after remeasure. A null rough opening signals the item has not been remeasured and is not ready to order.
- **Who measures it:** The contractor. Customers are not expected to know or provide this.

### 3. Outside Measurement

- **Definition:** The dimension of the window unit as seen from the exterior — the part that has to fit within the brick surround, vinyl cladding, or exterior opening.
- **Why it's variable:** The outside measurement depends on:
  - Whether the **new window** has brickmould (an exterior casing/trim piece that wraps the outside of the frame)
  - Whether the **existing/old window** had brickmould
  - How the original window was installed (e.g., nailed through brickmould into brick, or set into a vinyl surround)
- **Key scenario:** If the existing installation used brickmould nailed into brick, and the new window also has brickmould, the outside measurement is the brickmould-to-brickmould dimension. If you're replacing into a tight vinyl surround with no brickmould clearance, the outside measurement is the frame size. These are genuinely different numbers.
- **Computed from:** Frame size + brickmould profile geometry. The configurator already collects `brickmouldStyle` and `brickmouldColor` and derives `osmWidth`/`osmHeight` in the JavaScript pricing engine.
- **Who needs it:** Both contractors and customers, though for different reasons. A customer needs to know if their new window fits the hole; a contractor needs it to verify fit in the exterior cladding.

### Summary Table

| Measurement | Populated at | Can be null | Who enters it |
|---|---|---|---|
| Frame | Quote time | No | Contractor or prospect |
| Rough opening | Remeasure visit | Yes (until remeasured) | Contractor only |
| Outside | Quote time (computed) | No | Derived from brickmould config |

---

## Product Lines

A product line is a window series from a manufacturer. The three product lines currently in the system are all from **All Weather Windows (AWW)**, all **PVC construction**:

- **EnergySaver 2500** — baseline series
- **Apex** — mid-range
- **Carriage** — premium

Each product line defines:
- Manufacturer name
- Frame image assets (corners, edges, mullions) for SVG preview rendering
- Mullion width, enclosure width, mullion indents
- Frame size restrictions (min/max width/height)
- Available operational styles with their own size restrictions
- Default selections (jamb depth, pane configuration, frame color, brickmould, grille, SDL)

### Product Line Selection in Context

In real jobs, **product line is usually a project-level decision**, not a per-window decision. A contractor typically selects a product line for the whole job (or the CRM context provides it). Mixing product lines within a single job is possible for quoting purposes, but a **manufacturer's production order is per product line** — you cannot mix product lines on one purchase order. A quote with multiple product lines will result in multiple orders when converted to purchase.

The platform's `Tenant` entity has a `MixedProductLinesAllowed` policy flag to enforce or relax this at the quote level.

---

## Operational Styles

An operational style is how a window section opens (or doesn't). Each style has its own size restrictions independent of the frame restrictions.

Known styles in the current catalog:
- **Picture** — fixed, does not open
- **Fixed Sash** — fixed, similar to Picture in behavior
- **Awning** — hinges at the top, opens outward from the bottom (has crank)
- **Casement** — hinges at the side, opens outward (has crank)
- **Casement - Left** — casement hinged on the left
- **Horizontal Slider** — slides left/right

The `hasCrank` flag on a style indicates whether a crank operator is needed.

---

## Window Sections

A single window unit can have 1–3 horizontal sections (panes side by side). Each section is independently configured with:

- **Width and height** (fractional inches)
- **Row/col position** in the frame grid
- **Operational style**
- **Grille settings** — pattern, color, size, scope (current section / outside sections / all)
- **SDL settings** — simulated divided lite, same options as grille
- **Pane options**
- **Crank type** (if style has crank)

### Grille Patterns
Ladder, Double Ladder, Rectangular, Perimeter, Double Perimeter, Empress, None.

---

## Frame-Level Options

Set once for the whole window unit, not per section:

- **Frame color** — exterior color (e.g., White, Architectural Brown)
- **Jamb depth** — wall thickness accommodation (e.g., "4 Inches")
- **Pane configuration** — glass type (e.g., Dual, Triple pane)
- **Brickmould style** — exterior casing profile (e.g., None, or a specific profile)
- **Brickmould color** — exterior casing color
- **Sizing type** — how the window dimensions are entered

---

## Measurement System

The system uses **exact fractional inch arithmetic** throughout — no floating point. Measurements are represented as:

```
{ sign: 1 or -1, whole: integer, numerator: integer, denominator: integer }
```

Example: `34 5/8"` → `{ sign: 1, whole: 34, numerator: 5, denominator: 8 }`

This is critical for manufacturing tolerances. A 1/16" error in a window order is a real problem. Floating point cannot represent fractions like 5/8 or 3/16 exactly across addition/subtraction chains.

The JavaScript measurement library handles:
- Parsing strings like `"34 5/8"`
- Addition and subtraction of fractional measurements
- Decimal conversion for display
- Validation

---

## Pricing Model

Pricing is a **2D interpolation lookup**:

```
Manufacturer → Product Line → Style → Width breakpoints → Height breakpoints → Price per inch
```

The price-per-inch value is multiplied against a measure of the window (perimeter or area). Additional multipliers apply conceptually for:
- Grille option
- SDL option
- Pane count (Dual vs Triple)
- Pane configuration

The browser still computes a live estimate for responsiveness, but the server now computes the authoritative completion price and persists it. The remaining pricing work is policy-oriented: server validation must reject unsupported/out-of-range payloads before pricing, and past-final-breakpoint legacy extrapolation behavior is intentionally tracked by tests rather than treated as normal supported pricing.

---

## The Quote vs. Order Distinction

This is a critical business rule.

| | Quote | Order |
|---|---|---|
| Purpose | Estimate for customer, internal planning | Purchase order sent to manufacturer |
| Rough opening | Approximated or not yet known | Must be verified by remeasure |
| Mixed product lines | Allowed (with tenant policy) | One order per product line |
| Who creates | Contractor or prospect | Contractor only, after remeasure |
| Measurements trusted | Customer/contractor estimates | Contractor-verified on-site measurements |

A quote becomes orderable when:
1. The customer has approved the quote
2. A contractor has performed a remeasure and updated the rough opening measurements
3. All items are complete and validated

---

## The Two User Flows

### CRM-Initiated (Contractor / B2B)

1. CRM authenticates with the WindowConfigurator API
2. CRM POSTs a session stub (customer info, opportunity ID, product line context)
3. API returns a session URL
4. CRM opens that URL in a webview or iframe for the contractor
5. Contractor configures the window
6. On completion, API fires a webhook to the tenant's registered callback URL
7. CRM receives the configured item payload and handles it

The CRM is the system of record for the sales lifecycle. WindowConfigurator is the system of record for pre-submission configuration state.

### Website-Initiated (Prospect / B2C)

1. Prospect visits the dealer's website
2. Clicks "Build My Window" — no account required
3. Enters email address
4. Receives a magic link (passwordless token) via email
5. Clicks link, lands in configurator with a session tied to their email
6. Configures their window(s)
7. On completion, API fires a webhook to the dealer's registered callback URL
8. Dealer's CRM workflow receives the event and creates a lead/opportunity

The platform never calls the CRM API. It fires a generic webhook to a URL the dealer registered.

---

## Multi-Tenancy

Each client (window dealer or manufacturer) is a **Tenant**:

- Has API credentials for their CRM to authenticate
- Has a registered webhook callback URL
- Has a list of allowed product lines
- Has a `MixedProductLinesAllowed` policy flag
- Has branding configuration (logo, colors) for white-label embedding

The platform is completely CRM-agnostic. No Salesforce adapter, no HubSpot adapter. The tenant's CRM admin sets up the integration on their side. WindowConfigurator just exposes a clean API and fires webhooks.

---

## Session Lifecycle

```
Draft → Completed → Submitted
                  ↘ Expired (if session times out without completion)
```

A `QuoteSession` contains one or more `ConfiguredWindowItem` entries. Each item has its own `Draft → Completed` lifecycle. The session transitions to `Completed` when all items are completed, and the webhook fires at that point.

---

## Key Entities

### TenantEntity
- `Id`, `Name`, `ApiKey`, `WebhookCallbackUrl`
- `AllowedProductLineKeys` (list)
- `MixedProductLinesAllowed` (bool, default false)
- `Branding` (LogoUrl, PrimaryColor, AccentColor)
- `CreatedAt`

### QuoteSessionEntity
- `Id`, `TenantId`, `Status` (Draft/Completed/Submitted/Expired)
- `DefaultProductLineKey` (nullable hint, not enforced if mixed lines allowed)
- `CustomerEmail` (prospect flows)
- `ExternalReferenceId` (CRM's opportunity/record ID, echoed in webhook)
- `MagicLinkToken`, `MagicLinkExpiresAt` (prospect flows only)
- `CreatedAt`, `CompletedAt`, `ExpiresAt`
- `Items` (list of ConfiguredWindowItemEntity)

### ConfiguredWindowItemEntity
- `Id`, `QuoteSessionId`, `Status` (Draft/Completed)
- `Location` (e.g. "Master Bedroom Left"), `LineItemNumber`, `MeetsEgress`
- `ProductLineKey` (authoritative, resolved at item creation)
- **Frame:** `FrameWidthDecimal`, `FrameHeightDecimal`
- **Rough opening:** `RoughOpeningWidthDecimal`, `RoughOpeningHeightDecimal` (null until remeasured)
- **Outside:** `OutsideMeasureWidthDecimal`, `OutsideMeasureHeightDecimal`
- `SectionCount`
- `AuthoritativePrice` (decimal, server-computed — client price is ignored)
- `PricingComputedAt`
- `ConfigurationJson` (full Knockout payload blob — source for validation and webhook)
- `CreatedAt`, `CompletedAt`

---

## What Is Built vs. What Is Not

### Built and working
- Configurator UI (Knockout.js, Bootstrap 3)
- Fractional inch measurement library (JS)
- SVG live preview with real-time update
- Grille pattern geometry engine
- Client-side pricing calculator (2D interpolation)
- Client-side validation (size restrictions)
- Product catalog data (three product lines fully configured)
- ASP.NET Core MVC shell
- EF Core persistence for tenants, quote sessions, and configured window items
- Server-owned catalog resolution for item and section templates
- Server-authoritative pricing at completion, including persisted authoritative item prices
- Server-side completion validation for tenant product-line access, frame/style constraints, option availability, and pricing-grid limits

### Stubbed / placeholder
- The current MVC completion endpoint is a bridge until the versioned API surface exists
- `/` falls back to the first development session when no explicit session ID is provided
- Draft save is intentionally deferred until the UI supports adding and switching between multiple items

### Not yet built
- Secured external API layer (`/api/v1/...`) — minimal quote-session create/get/update, item add/update, and submit endpoints are present, but auth and production contract hardening remain
- Authentication (API keys, magic links)
- Full webhook dispatch system (retry/backoff and durable delivery tracking remain)
- Full multi-tenant hardening
- Email delivery

---

## Implementation Roadmap Summary

0. Stabilize demo surface — remove fake UI affordances ✅
1. Define real backend shape — domain models and session contracts ✅
2. Introduce session persistence (EF Core + database) ✅
3. Move catalog resolution server-side ✅
4. Port authoritative pricing to C# ✅
5. Add server-side validation at completion ✅
6. Add minimal REST API surface ✅
7. Add CRM handoff and webhook dispatch 🚧
8. Harden for real multi-tenant use
9. (Optional) Frontend modernization

The guiding principle: **backend-first**. The configurator UI works. Make the server authoritative before touching the frontend.
