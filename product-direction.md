# WindowConfigurator Product Direction

## Purpose

WindowConfigurator supports a window contractor and homeowner through a shared
configuration workflow. The contractor relationship remains primary: the
platform helps capture preferences, measurements, and an order-ready
configuration; it does not replace the contractor, CRM, manufacturer, or
payment process.

## Entry flows

### Contractor-led

A contractor captures an exterior photo and spoken window specification during
an on-site assessment. The contractor reviews the estimated dimensions and
parsed configuration, creates a session, and shares a customer link. The
customer can review the configuration and change aesthetic choices. A later
physical remeasure confirms the dimensions before contractor sign-off.

### Customer-led

A customer can configure preferences online and optionally provide indicative
sizing. The contractor receives the inquiry, performs an on-site assessment,
and the session then follows the contractor-led flow.

## Measurement and order readiness

Every session item distinguishes its measurement confidence:

| Source | Customer-facing pricing state | Order readiness |
|---|---|---|
| Customer-entered | Indicative estimate | Not order-ready |
| Image estimate | Site-photo estimate | Not order-ready |
| Physical confirmation | Confirmed price | Order-ready |

A contractor-provided measurement supersedes a customer-entered estimate.
After contractor measurement, sizing remains under contractor authority while
customers can continue to change non-size attributes such as finish, hardware,
grille pattern, and glass options.

Physical remeasure uses interior frame and exterior brick-mold measurements.
It is the basis for manufacturing accuracy and order readiness; an image
estimate remains an estimate.

## Product boundaries

- The platform sends generic outbound webhooks to tenant-configured endpoints;
  it does not call CRM APIs.
- Pricing presented during configuration responds locally from a preloaded
  pricing grid. The server remains responsible for validating submitted
  configurations and recording authoritative results.
- Measurements use exact fractional values rather than floating point.
- A mixed-product-line session becomes separate manufacturer orders at
  conversion.
- Payment processing, manufacturer communication, installation, and CRM
  pipeline management are outside the platform boundary.

## Source history

This document carries forward product conclusions from the archived 2026
discovery records. The complete source material, including diagrams,
requirements, validation hypotheses, and their historical context, is retained
in [the governance reset archive](./archive/2026-11-agent-governance-reset/README.md).
The accepted architectural rationale remains in [adr/](./adr/).
