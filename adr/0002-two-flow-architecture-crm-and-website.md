# ADR 0002 — Two-Flow Architecture: CRM-Initiated and Website-Initiated

**Date:** 2026-05-11
**Status:** Accepted

---

## Context

The platform needs to support two fundamentally different entry points for the configurator:

1. A contractor already working in a CRM on an active opportunity, who needs to spec out a window for a customer.
2. A prospect on a dealer's website, with no account and no CRM context, who wants to explore options and get a price.

These two users have different identity models, different levels of technical knowledge, different session-initiation mechanisms, and different downstream expectations. The question was whether to build two separate flows or find a unified model.

---

## Decision

**Build two distinct flows that share the same underlying platform, the same session model, and the same unidirectional webhook pattern.**

### Flow 1 — CRM-Initiated (Contractor / B2B)

1. The CRM authenticates with the platform API using tenant API credentials.
2. The CRM POSTs a session stub containing customer info, an external reference ID (opportunity/record), and optionally a pre-selected product line.
3. The API creates a `QuoteSession` and returns a session URL.
4. The CRM opens that URL in a webview or iframe for the contractor.
5. The contractor configures the window.
6. On completion, the API fires a webhook to the tenant's registered callback URL with the completed payload.
7. The CRM handles the payload however it wants.

### Flow 2 — Website-Initiated (Prospect / B2C)

1. The prospect visits the dealer's website and clicks a "Build My Window" entry point.
2. The prospect enters their email address. No account is required.
3. The platform generates a magic link (passwordless session token) and sends it to the email.
4. The prospect clicks the link and lands in the configurator with a session tied to their email.
5. The prospect configures their window(s).
6. On completion, the API fires a webhook to the tenant's registered callback URL.
7. The dealer's CRM creates a lead or opportunity from the payload.

---

## Alternatives Considered

**Single unified flow with optional CRM context (rejected)**
One entry point that optionally accepts CRM parameters. Rejected because the authentication models are fundamentally different — machine-to-machine API key auth for CRM, magic link for prospects — and forcing them into one flow adds complexity without real benefit.

**Full account system for prospects (rejected)**
Require prospects to create an account before configuring. Rejected because it creates friction at the top of the funnel. The prospect has not committed to anything yet. A magic link gives them a persistent session without the overhead of account creation, and it captures their email in a low-friction way.

**CRM-initiated flow only (rejected)**
Ignore the website prospect flow. Rejected because it limits the platform to B2B-only use and removes a significant source of inbound leads for dealers.

---

## Consequences

- Both flows use the same `QuoteSession` and `ConfiguredWindowItem` entities. The session knows which flow created it (`MagicLinkToken` null = CRM-initiated; non-null = website-initiated).
- Both flows use the same unidirectional webhook dispatch (ADR 0001).
- The platform needs two authentication mechanisms: API key / OAuth client credentials for machine clients, and magic link tokens for human prospects.
- The completion payload is identical regardless of flow. The CRM integration point does not need to know which flow originated the session.
- Magic link tokens must expire. Sessions must also expire if abandoned. Both are handled on the `QuoteSession` entity (`MagicLinkExpiresAt`, `ExpiresAt`).
