# ADR 0016 — Iframe Embedding as Configurator Integration Pattern

**Date:** 2026-05-22
**Status:** Accepted

---

## Context

Both integration flows (ADR 0002) require the host system to surface the configurator to the end user after a session has been created. The configurator is a full web application with its own routing, JavaScript state, and Knockout-based UI. The host system (a contractor's website or a CRM embedded view) needs to present this application without losing its own context.

Three options exist for how the host presents the configurator after receiving a session URL:

1. Full-page redirect to the configurator URL
2. Opening the configurator in a popup or new tab
3. Embedding the configurator in an iframe within the host page

---

## Decision

**Embed the configurator via an iframe.**

The host page creates an iframe element and sets its `src` to the session launch URL returned by `POST /api/v1/quote-sessions`. The host page remains visible and in context for the entire session.

Session creation flow:
1. User clicks a CTA or "Start Quote" button on the host page
2. Host page JavaScript calls the host's own server-side endpoint (`/api/session-launch` on MockContractorSite, `/api/opportunities/{id}/start-quote` on MockContractorCrm)
3. Host server calls `POST /api/v1/quote-sessions` with the API key (server-to-server, key never reaches the browser)
4. Host server returns the absolute launch URL to the browser
5. Browser sets `iframe.src` to the launch URL
6. Configurator loads inside the iframe; host page chrome remains visible

---

## Alternatives Considered

**Full-page redirect to the configurator URL (rejected)**
The host page is replaced entirely by the configurator. On completion, the configurator would need to redirect back to the host. Rejected because it requires the configurator to know the host's return URL, breaks the host page's navigation context, and loses any host page state (customer info, open CRM record, etc.) for the duration of the session.

**Popup / new tab (rejected)**
Open the configurator URL in a separate window. Rejected because modern browsers block popups that are not directly triggered by a user gesture in the same callstack, and popup blocking degrades the demo experience unpredictably. New-tab navigation loses the visual association with the host page, making the flow feel disconnected.

**Client-side session creation (rejected)**
Have the browser call `POST /api/v1/quote-sessions` directly, then set the iframe src. Rejected because `X-Api-Key` must never be exposed in browser JavaScript — it is a server-to-server credential. Server-side session bootstrap is not negotiable (see ADR 0011).

---

## Consequences

- The host page is always visible around the configurator, which is appropriate for both the contractor CRM flow (operator sees the opportunity record while configuring) and the website flow (prospect sees the dealer's branding throughout).
- iframe `src` manipulation is simple: one JavaScript assignment after the host server responds. No routing library or framework needed on the host.
- Completion signaling from the configurator to the host must cross the iframe boundary. The browser same-origin policy prevents direct DOM access between frames on different origins. This is addressed by ADR 0017 (postMessage).
- The configurator application must not assume it is always in a top-level frame. Code that assumes `window === window.top` will behave incorrectly when embedded. Error handling and navigation within the configurator must account for the iframe context.
- iframe embedding is the established pattern for embedded payment forms, chat widgets, and configuration tools. This is not novel; it is the standard approach for third-party embedded UI.
