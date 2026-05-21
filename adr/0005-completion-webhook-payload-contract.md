# ADR 0005 — Completion Webhook Payload Contract

**Date:** 2026-05-17
**Status:** Accepted

---

## Context

When a quote session completes, the platform fires a webhook POST to the tenant's registered callback URL (see ADR 0001). The receiving system — typically the tenant's CRM or a middleware integration layer — builds a workflow against this payload. Once consumers are building against a payload shape, breaking changes are expensive.

This ADR defines the payload contract, the guarantees made about it, and the decisions made about its structure.

---

## Decision

The completion webhook POSTs a `QuoteCompletedPayload` to the tenant's callback URL with `Content-Type: application/json`.

### Guaranteed Fields

These fields are always present and non-null when the event fires:

- `eventType` — always `"quote.completed"`
- `occurredAt` — ISO 8601 UTC timestamp
- `session.id` — the platform's session GUID
- `session.tenantId`
- `session.completedAt`
- `session.itemCount`
- `orderGroups` — at least one group, each with at least one item
- Per item: `id`, `lineItemNumber`, `location`, `meetsEgress`, `sectionCount`
- Per item measurements: `frame.widthInches`, `frame.heightInches`, `outside.widthInches`, `outside.heightInches`
- Per item measurements: display strings (`frame.widthDisplay`, etc.) in fractional inch format

### Conditionally Present Fields

- `session.externalReferenceId` — present for CRM-initiated sessions; null for website-initiated unless the CRM provided it
- `session.customerEmail` — present for website-initiated sessions; null for CRM-initiated unless provided
- `measurements.roughOpening` — null until the contractor performs a remeasure visit; a null rough opening means the window is not yet ready to order
- `authoritativePrice` (item and group level) — present after server-side pricing has run; null only when a session/item has not yet been priced
- `session.totalAuthoritativePrice` — present after server-side pricing has run; null only when aggregate pricing has not yet been computed

### Order Groups

Items are grouped by product line in `orderGroups`. Most quotes will have one group. Mixed-product-line quotes produce multiple groups. Each group corresponds to one manufacturer purchase order.

---

## Sample Payload

```json
{
  "eventType": "quote.completed",
  "occurredAt": "2026-05-17T21:45:00Z",
  "session": {
    "id": "a1b2c3d4-0000-0000-0000-000000000001",
    "tenantId": "t1000000-0000-0000-0000-000000000001",
    "externalReferenceId": "CRM-OPP-00492",
    "customerEmail": null,
    "completedAt": "2026-05-17T21:44:58Z",
    "itemCount": 2,
    "totalAuthoritativePrice": 1066.75
  },
  "orderGroups": [
    {
      "productLineKey": "energysaver-2500",
      "productLineName": "EnergySaver 2500",
      "manufacturerName": "All Weather Windows",
      "groupAuthoritativePrice": 1066.75,
      "items": [
        {
          "id": "i1000000-0000-0000-0000-000000000001",
          "lineItemNumber": 1,
          "location": "Master Bedroom Left",
          "meetsEgress": false,
          "sectionCount": 2,
          "authoritativePrice": 544.10,
          "measurements": {
            "frame": {
              "widthInches": 70.25,
              "heightInches": 45.0625,
              "widthDisplay": "70 1/4",
              "heightDisplay": "45 1/16"
            },
            "roughOpening": null,
            "outside": {
              "widthInches": 72.25,
              "heightInches": 47.0625,
              "widthDisplay": "72 1/4",
              "heightDisplay": "47 1/16"
            }
          },
          "configuration": { "...": "full configurator snapshot" }
        },
        {
          "id": "i1000000-0000-0000-0000-000000000002",
          "lineItemNumber": 2,
          "location": "Kitchen",
          "meetsEgress": true,
          "sectionCount": 1,
          "authoritativePrice": 522.65,
          "measurements": {
            "frame": {
              "widthInches": 36.0,
              "heightInches": 48.0,
              "widthDisplay": "36",
              "heightDisplay": "48"
            },
            "roughOpening": null,
            "outside": {
              "widthInches": 36.0,
              "heightInches": 48.0,
              "widthDisplay": "36",
              "heightDisplay": "48"
            }
          },
          "configuration": { "...": "full configurator snapshot" }
        }
      ]
    }
  ]
}
```

---

## Alternatives Considered

**Flat item list, no order group wrapper (rejected)**
Simpler, but would require a breaking change when a mixed-product-line quote needs to produce multiple purchase orders. Grouping now costs almost nothing; removing grouping later would break all consumers.

**Include full normalized section data at the top level of each item (rejected)**
Promote section-level fields (style name, grille pattern, etc.) out of `configuration` into typed fields. Rejected for now — the receiving CRM typically doesn't need to parse individual section choices. The full snapshot in `configuration` is available if needed. This can be promoted in a later payload version without breaking existing consumers.

**Separate webhook events per order group (rejected)**
Fire one webhook per product line group. Rejected because most sessions have one group, and splitting events introduces a coordination problem for consumers who need a complete picture of the session. One event per session completion is the right granularity.

---

## Consequences

- The `configuration` blob inside each item is the full Knockout payload. Consumers should treat it as opaque unless they specifically need section-level detail.
- `roughOpening: null` is a meaningful signal — the CRM integration should check for null rough openings before treating a quote as order-ready.
- Consumers must still handle null prices gracefully for draft or unpriced records, but completed sessions are expected to include server-computed authoritative prices.
- This contract is versioned implicitly by the platform version. Future breaking changes will be introduced via a versioned API path, not by modifying this payload in place.
