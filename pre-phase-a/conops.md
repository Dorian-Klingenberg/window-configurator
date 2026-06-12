# Concept of Operations — WindowConfigurator

*Pre-Phase A artifact · Version 1.0 · 2026-06-12*

---

## 1. System Overview

WindowConfigurator is a two-sided window replacement platform connecting window contractors and their customers through a shared configuration and quoting workflow. The system manages the full journey from initial contact to signed order, with measurement accuracy and pricing confidence increasing at each stage.

The platform is contractor-branded — contractors embed or link to it from their own web presence. It is not a marketplace. The contractor relationship is primary; the customer transacts through the contractor, not around them.

---

## 2. System Boundary

**Inside the system:**
- Window configuration (type, style, finish, hardware, grille pattern, sizing)
- Measurement confidence tracking (source and accuracy tier for each dimension)
- Pricing calculation, qualified by measurement confidence tier
- Session lifecycle management (draft → customer review → confirmed → order-ready)
- Customer-facing configuration link (contractor-issued, contractor-branded)
- Image processing pipeline for exterior measurement estimation
- Contractor portal for session management and customer handoff

**Outside the system:**
- CRM systems (contractors integrate via outbound webhooks; the platform never calls a CRM)
- Physical site visits and remeasure logistics
- Order fulfillment and manufacturer communication
- Payment processing
- Window installation

---

## 3. Users and Stakeholders

| Role | Description |
|---|---|
| **Window contractor** | Primary user. Manages sessions, issues customer links, performs remeasures, places orders. |
| **Customer / homeowner** | Secondary user. Reviews contractor-generated configurations, customizes non-size attributes, communicates preferences back to contractor. |
| **Manufacturer** | Downstream recipient of completed orders (outside system boundary). |

---

## 4. Operational Scenarios

### Scenario A — Contractor-Led Flow (On-Site Initiation)

This is the primary flow. The contractor is the first mover.

1. **Site visit:** Contractor visits the customer's property for an initial assessment.
2. **Photo capture:** Contractor photographs each window from the exterior using the contractor portal or mobile interface.
3. **Image processing estimate:** The system analyzes the exterior photo and produces an estimated rough opening for each window. This is estimate-grade accuracy — sufficient for configuration and indicative pricing, not for order placement.
4. **Window specification:** Contractor selects the window type for each opening (e.g., right casement, left casement, double-hung) and confirms the image-derived dimensions or adjusts them.
5. **Session creation:** The system creates a configuration session per window (or per project) with sizing locked to the contractor. Pricing is calculated and labeled as an estimate.
6. **Customer link issued:** The contractor sends the customer a link to their session. The customer can view the configuration and pricing, and modify any attribute that does not affect fit — finish, hardware, grille pattern, glass options, etc. Sizing fields are read-only.
7. **Customer review:** The customer explores the configuration, sees how their choices affect pricing, and communicates their preferences to the contractor. This is not a quote acceptance — it is a preference declaration.
8. **Final remeasure scheduled:** The customer and contractor agree on a direction. The contractor schedules a second visit.
9. **Physical remeasure:** On the second visit, the contractor removes interior casing and takes a precise interior frame measurement, and a precise exterior brick mold measurement. These replace the image-processing estimate. Pricing updates to final (confirmed) status.
10. **Order placement:** Papers are signed. The system produces an order-ready configuration. The order is placed with the manufacturer.

### Scenario B — Customer-Led Flow (Online Initiation)

This is the secondary flow. The customer is the first mover.

1. **Discovery:** The customer finds the contractor online and visits the contractor's website, which contains a link to a WindowConfigurator-powered configurator.
2. **Self-configuration:** The customer configures their window preferences — type, style, finish, hardware — and optionally enters their own sizing estimates. The system accepts customer-entered sizing but labels the resulting pricing as indicative only.
3. **Submission:** The customer submits the configuration as an inquiry to the contractor. The system notifies the contractor via webhook.
4. **Contractor review:** The contractor reviews the customer's configuration and preferences, uses it as a briefing document for the initial visit, and schedules a site visit.
5. **Merge into Scenario A:** From the site visit onward, the flow is identical to Scenario A from step 2. The contractor's image-processing estimate supersedes the customer's self-entered sizing with a higher-confidence measurement.

---

## 5. Measurement Confidence Model

Measurement source is a first-class concept in the system. Every sizing value carries a source, and the source determines pricing accuracy and order-readiness.

| Measurement source | Confidence tier | Pricing label | Order-ready |
|---|---|---|---|
| Customer-entered | Indicative | "Indicative estimate — subject to survey" | No |
| Image-processing estimate | Estimate | "Estimate — based on site photo" | No |
| Contractor physical remeasure | Confirmed | "Confirmed price" | Yes |

Pricing is always computable at any stage. The label changes; the calculation engine does not.

When the contractor provides an image estimate or physical remeasure, it supersedes the previous measurement source. The customer cannot override a contractor-provided measurement.

---

## 6. Key Operational Constraints

- **Contractor-locked sizing:** Once a contractor has provided a measurement (image estimate or physical), the customer cannot modify sizing. The contractor is the authoritative measurement source.
- **No CRM API calls:** The platform fires outbound webhooks. It never calls a CRM directly.
- **No floating-point measurements:** All window dimensions use exact fractional arithmetic (sign, whole, numerator, denominator). A 1/16" error is a manufacturing defect.
- **One manufacturer order per product line:** Mixed product lines in a session produce separate orders at conversion.
- **Rough opening null until contractor measures:** Order placement requires a confirmed (physical remeasure) rough opening. Image estimates and customer entries do not satisfy this gate.
