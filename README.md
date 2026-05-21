# WindowConfigurator

## What It Is

A **B2B SaaS window configuration platform** for the building/renovation industry. The core product is a rich browser-based window configurator that contractors or prospects use to spec out replacement or new windows — selecting product lines, dimensions, operational styles, finishes, grilles, and glass options — and generate a priced order item. The business model is **white-label / embedded**: window manufacturers or dealers embed this system into their sales workflow via their CRM of choice.

---

## The Architecture Philosophy

The system is deliberately **CRM-agnostic** and the integration model is **unidirectional — the CRM calls us, not the other way around**. This was a conscious design decision to avoid writing CRM-specific adapters (Salesforce, HubSpot, etc.) and to keep the platform generic. The CRM expert on the client's side owns the integration — they set up the flows, the field mappings, the automation. This system is just a service they call.

The platform is the **source of truth for order item configuration data**. The CRM is a downstream consumer.

---

## Two Distinct User Flows

### Flow 1 — CRM-Initiated (B2B / Contractor)
1. CRM authenticates with the WindowConfigurator API
2. CRM POSTs an order item stub (customer info, opportunity ID, line item context)
3. API returns a session URL
4. CRM opens that URL in a webview or iframe for the contractor
5. Contractor configures the window in the configurator UI
6. On completion, configurator POSTs the result back to the API
7. API fires a webhook to a pre-registered callback URL on the CRM side
8. CRM receives the configured item data and does whatever it wants with it

> **Key point:** WindowConfigurator never calls the CRM API. It fires a generic webhook to a URL the CRM admin registered. The CRM side handles interpretation.

### Flow 2 — Website-Initiated (B2C / Prospect)
1. Prospect visits the window dealer's website
2. Clicks "Build My Window" — no account required
3. Enters their email address
4. Receives a **magic link** (passwordless login token) via email
5. Clicks link, lands in the configurator with a session tied to their email
6. Configures their window
7. On completion, result is saved to the WindowConfigurator system
8. System fires a webhook to the dealer's registered callback URL
9. The dealer's CRM workflow receives the event and creates a lead/opportunity

> **Key point:** Same unidirectional webhook model. The platform never needs to know what CRM the dealer uses. The prospect flow stays clean and adapter-free.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core MVC, .NET 10 |
| Frontend (configurator) | Knockout.js 3.4.1, knockout.mapping, jQuery, Bootstrap 3 |
| Preview rendering | SVG, generated via Knockout computed observables |
| Persistence (current) | EF Core with SQLite for local development |
| Product catalog | Server-resolved manifest plus flat JSON assets in AppData |
| Pricing data | JSON lookup tables with 2D width/height breakpoints, loaded by the server pricing service |

---

## Current State of the Codebase

### What's Done / Mature
- **The configurator engine** — this is the complex part and it works. It was originally built for a different commerce platform (NALP commerce) and has been partially ported to ASP.NET Core
- **Fractional inch measurement system** — custom JS library handling sign/whole/numerator/denominator arithmetic, parsing strings like `"34 5/8"`, validation, add/subtract, decimal conversion
- **SVG live preview** — Knockout-bound SVG that renders the window frame, mullions, brickmould, and section panes using tiled image patterns. Updates in real time as the user configures
- **Grille pattern geometry engine** — generates SVG line sets for patterns: Ladder, Double Ladder, Rectangular, Perimeter, Double Perimeter, Empress
- **Pricing calculator** — browser still provides a live estimate, while the server computes and persists the authoritative completion price
- **Validation** — browser checks remain for UX, and server-side completion validation now enforces tenant product-line policy, frame/style restrictions, option availability, and pricing-grid support before completion is accepted
- **Product catalog data** — three product lines fully configured: EnergySaver 2500, Apex, Carriage (all from All Weather Windows, all PVC)
- **Session persistence** — EF Core stores tenants, quote sessions, configured window items, JSON snapshots, completion status, and authoritative prices
- **Server catalog resolution** — `OrderItemController` resolves item and section templates from session/product-line context instead of trusting arbitrary client filenames
- **Minimal versioned API surface** — `/api/v1/quote-sessions` now supports create/get/update, item add/update, and submit with shared API error envelopes
- **Webhook dispatch kickoff** — session submit now triggers a `quote.completed` webhook attempt and returns delivery attempt metadata

### What's Stubbed / Placeholder
- The MVC completion path is still a pragmatic bridge, not the final versioned `/api/v1/...` surface
- The versioned API surface now includes quote-session create/get/update, item add/update, and session submit, but authentication, payload hardening, and full integration contracts are still in progress
- `/` falls back to the first available development session; production entry points must pass explicit session IDs
- Draft save is intentionally absent until the UI supports adding and switching between multiple items

### What Doesn't Exist Yet
- Secured external API layer (authentication/authorization and production integration hardening for CRM workflows)
- Authentication / authorization (no JWT validation, no API keys, no magic link system)
- Full webhook delivery system (durable delivery logging, retry/backoff, and failure recovery)
- Full multi-tenant hardening (per-client auth, tenant isolation at every entry point, production branding/admin workflows)
- Email delivery
- Any outbound CRM calls (intentionally — by design)

---

## Domain Model

### Order
Belongs to a client tenant and optionally a CRM opportunity. Has a customer (identified by email for prospects, by CRM record for contractor flows). Contains one or more **Order Items**.

### Order Item
A single window being configured. Key fields:
- Location (e.g. "Master Bedroom Left")
- Line item number
- Meets egress flag
- **Product Line** (the window series)
- **Sections** (1–3 horizontal panes)
- Frame-level properties: frame color, brickmould style/color, jamb depth, pane configuration, sizing type

### Product Line
Defines a window series. Contains:
- Manufacturer name
- Frame image assets (corners, edges, mullion) for SVG rendering
- Mullion width, enclosure width, mullion indents
- Frame size restrictions (min/max width/height)
- List of **Operational Styles**

### Operational Style
How a section opens. Examples: Picture (fixed), Awning, Casement, Horizontal Slider. Each style has:
- Its own image assets
- Its own size restrictions (independent of frame restrictions)
- Slider image for animated open/close preview
- hasCrank flag

### Section
One pane of the window. Has:
- Row/col position in the frame grid
- Width and height (fractional inches)
- Assigned operational style
- Grille settings (pattern, color, size, scope)
- SDL settings (simulated divided lite — pattern, color, size, scope)
- Pane options

### Measurement
The fundamental unit throughout the system. Represented as:
- sign (1 or -1)
- whole (integer)
- numerator (integer)
- denominator (integer)

This allows exact fractional arithmetic without floating point errors — critical for window manufacturing where 1/16" matters.

---

## Pricing Model

Pricing is a **2D interpolation lookup** structured as:

```
Manufacturer → Product Line → Style → Width Breakpoints → Height Breakpoints → Price Per Inch
```

The price-per-inch value is multiplied against some measure of the window (perimeter or area). Additional multipliers exist conceptually for grille, SDL, pane count, and pane configuration but were not fully implemented.

---

## What Needs To Be Built (Priority Order)

### 1. External API Layer
Versioned REST API (`/api/v1/...`) with proper request/response DTOs. Key endpoints:
- `POST /api/v1/quote-sessions` — CRM creates a configurator session, gets back a URL
- `GET /api/v1/quote-sessions/{id}` — retrieve session state
- `PUT /api/v1/quote-sessions/{id}` — update draft state
- `POST /api/v1/quote-sessions/{id}/items` — add an item
- `PUT /api/v1/quote-sessions/{id}/items/{itemId}` — update an item
- `POST /api/v1/quote-sessions/{id}/complete` — complete a session

### 2. Authentication
- **API key or OAuth client credentials** for CRM-to-API authentication (B2B flow)
- **Magic link / passwordless token** for prospect authentication (B2C flow)
- ASP.NET Core's built-in token generation handles the magic link cleanly without needing full Identity

### 3. Webhook Dispatch
When a session completes, fire a POST to the tenant's registered callback URL with the configured item payload. Needs retry logic (exponential backoff), failure logging, and ideally a dead letter queue for failed deliveries.

### 4. Multi-Tenancy
Each client (window dealer) gets a tenant record with:
- API credentials
- Registered webhook callback URL(s)
- Branding config (logo, colors for white-label embedding)
- Which product lines they have access to

### 5. Continue Validation Hardening
Server-side validation is now authoritative at completion. Continue adding deeper compatibility rules and richer error modeling as needed while API and integration work proceeds.

### 6. Configurator Modernization
The Knockout.js / Bootstrap 3 frontend works but is dated. Long-term this probably moves to a modern JS framework after backend contracts stabilize.

---

## Running Locally

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or VS Code with C# Dev Kit extension

### Visual Studio
Open `WindowConfigurator.sln` and press F5.

### VS Code
Open the solution root folder and press F5. Choose **C# + JS (Edge)** or **C# + JS (Chrome)** from the debug panel. The browser will open automatically when the app is ready.

### CLI
```bash
cd WindowConfigurator
dotnet run
```

App runs at `http://localhost:5149` by default.

---

## Build Lessons

Phase-by-phase teaching documents (with code snippets and Mermaid diagrams) live in [`lessons/`](./lessons/README.md).

---

## Portfolio Notes

This project demonstrates several sophisticated, real-world engineering concepts:

- **CRM-agnostic B2B integration design** — webhook-based, unidirectional, adapter-free
- **Dual authentication patterns** — API key/OAuth for machine clients, magic link for human prospects
- **Domain-specific measurement systems** — exact fractional arithmetic for manufacturing tolerances
- **Complex client-side rendering** — SVG generation, MVVM pattern, real-time preview
- **Multi-tenant SaaS architecture**
- **Event-driven decoupling** — the system fires events, consumers decide what to do

The domain is real (window manufacturing has genuine complexity), the problem is real (CRM integration for trades businesses is a genuine pain point), and the architecture decisions are defensible and interesting to explain.
