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
| Persistence (current) | In-memory repository (placeholder) |
| Product catalog | Flat JSON files in AppData folder |
| Pricing data | JSON lookup tables with 2D width/height breakpoints |

---

## Current State of the Codebase

### What's Done / Mature
- **The configurator engine** — this is the complex part and it works. It was originally built for a different commerce platform (NALP commerce) and has been partially ported to ASP.NET Core
- **Fractional inch measurement system** — custom JS library handling sign/whole/numerator/denominator arithmetic, parsing strings like `"34 5/8"`, validation, add/subtract, decimal conversion
- **SVG live preview** — Knockout-bound SVG that renders the window frame, mullions, brickmould, and section panes using tiled image patterns. Updates in real time as the user configures
- **Grille pattern geometry engine** — generates SVG line sets for patterns: Ladder, Double Ladder, Rectangular, Perimeter, Double Perimeter, Empress
- **Pricing calculator** — fetches pricing JSON and interpolates across a 2D grid of width/height breakpoints with a price-per-inch model, per manufacturer/product line/style
- **Validation** — section width/height checked against per-style restrictions and overall frame restrictions
- **Product catalog data** — three product lines fully configured: EnergySaver 2500, Apex, Carriage (all from All Weather Windows, all PVC)

### What's Stubbed / Placeholder
- `OrderItemEntity` is nearly empty (just a Guid) — the rich configuration data has no C# model counterpart yet
- `OrderItemController.GetOverview()` always returns the EnergySaver template regardless of ID
- `SectionTemplate()` always returns the EnergySaver section template regardless of templateName parameter
- Salesforce user IDs are hardcoded strings in the controller
- No save flow — configured window data never POSTs back to the server

### What Doesn't Exist Yet
- External-facing API layer (no versioned, secured REST endpoints for CRM consumption)
- Authentication / authorization (no JWT validation, no API keys, no magic link system)
- Webhook registration and dispatch system
- Multi-tenant configuration (per-client CRM callback URLs, branding, etc.)
- Real database / persistence
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

### 1. Persistence Layer
Replace the in-memory repository with a real database. Entity Framework Core + SQL Server or PostgreSQL. The `OrderItemEntity` needs to be fleshed out to capture the full configured state (or store the JSON blob).

### 2. External API Layer
Versioned REST API (`/api/v1/...`) with proper request/response DTOs. Key endpoints:
- `POST /api/v1/sessions` — CRM creates a configurator session, gets back a URL
- `GET /api/v1/sessions/{token}` — retrieve session state
- `POST /api/v1/sessions/{token}/complete` — configurator posts final config (internal)
- `POST /api/v1/webhooks` — register a callback URL (tenant setup)

### 3. Authentication
- **API key or OAuth client credentials** for CRM-to-API authentication (B2B flow)
- **Magic link / passwordless token** for prospect authentication (B2C flow)
- ASP.NET Core's built-in token generation handles the magic link cleanly without needing full Identity

### 4. Webhook Dispatch
When a session completes, fire a POST to the tenant's registered callback URL with the configured item payload. Needs retry logic (exponential backoff), failure logging, and ideally a dead letter queue for failed deliveries.

### 5. Multi-Tenancy
Each client (window dealer) gets a tenant record with:
- API credentials
- Registered webhook callback URL(s)
- Branding config (logo, colors for white-label embedding)
- Which product lines they have access to

### 6. Configurator Modernization
The Knockout.js / Bootstrap 3 frontend works but is dated. Long-term this probably moves to a modern JS framework. Short-term, fixing the stubbed endpoints (load correct templates by product line, save configured state) is enough to make it functional end-to-end.

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

## Portfolio Notes

This project demonstrates several sophisticated, real-world engineering concepts:

- **CRM-agnostic B2B integration design** — webhook-based, unidirectional, adapter-free
- **Dual authentication patterns** — API key/OAuth for machine clients, magic link for human prospects
- **Domain-specific measurement systems** — exact fractional arithmetic for manufacturing tolerances
- **Complex client-side rendering** — SVG generation, MVVM pattern, real-time preview
- **Multi-tenant SaaS architecture**
- **Event-driven decoupling** — the system fires events, consumers decide what to do

The domain is real (window manufacturing has genuine complexity), the problem is real (CRM integration for trades businesses is a genuine pain point), and the architecture decisions are defensible and interesting to explain.
