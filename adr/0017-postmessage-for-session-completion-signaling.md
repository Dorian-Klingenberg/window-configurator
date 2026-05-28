# ADR 0017 — postMessage for Session Completion Signaling to Host Frames

**Date:** 2026-05-27
**Status:** Accepted

---

## Context

When the configurator is embedded in an iframe (ADR 0016), the host page needs to know when the user has finished configuring — specifically when a session reaches `Completed` status — so it can show a confirmation UI, hide the iframe, or trigger its own downstream actions.

The configurator and its host page are on different origins (e.g., `http://localhost:5149` vs. `http://localhost:5151`). The browser same-origin policy prevents the iframe from directly reading or writing DOM state on the parent page and prevents the parent page from reading the iframe's DOM state. A cross-origin communication mechanism is required.

This decision is scoped to the browser-to-browser communication path. The server-to-server webhook delivery path (ADR 0001, ADR 0008) is a separate concern and is not affected by this decision.

---

## Decision

**When a session completes, the configurator emits a `window.parent.postMessage` call.**

Triggering condition: the configurator's item-submit handler receives a server response with `sessionStatus === "Completed"`.

Message shape:
```json
{
  "type": "window.configurator.session.completed",
  "sessionId": "<guid>",
  "authoritativePrice": 544.10,
  "completedAt": "2026-05-27T18:30:00Z"
}
```

The call is wrapped in a `try/catch`. If the configurator is running in a top-level frame (not embedded), `window.parent.postMessage` still succeeds but has no listener; the call is silently ignored. This allows the same configurator build to work in both embedded and standalone contexts.

The host page registers a `window.addEventListener("message", ...)` listener that:
1. Ignores messages with unrecognized `type` values
2. Reads `sessionId`, `authoritativePrice`, and `completedAt` from the message data
3. Shows a completion confirmation panel with those values

Target origin is `"*"` (wildcard) in the current implementation.

---

## Accepted Risk: Wildcard Origin

Using `"*"` as the target origin means the message is delivered regardless of what page is hosting the iframe. A malicious page embedding the configurator would receive the message.

This is acceptable at demo stage because:
- The message contains no credentials or secrets; it contains only data already visible to the authenticated user who just completed the session
- The configurator itself is not a high-security surface at demo stage

In a production deployment, the target origin should be tightened to the specific tenant domain (e.g., `"https://dealer.example.com"`). The configurator would need to receive the allowed parent origin at session creation time and store it for use in the postMessage call. This change can be made without altering the message shape or the host-side listener.

---

## Alternatives Considered

**Server-side webhook to notify the host page (rejected)**
The webhook (ADR 0001) fires server-to-server. It reaches the CRM backend, not the browser session the user is currently viewing. A webhook cannot directly update a browser DOM. The webhook and postMessage serve different consumers and are not alternatives to each other.

**Host page polls the Configurator API for session status (rejected for website flow)**
The host page JavaScript could poll `GET /api/v1/quote-sessions/{id}` on an interval. Rejected because: (1) the API key is a server-to-server credential that must not be exposed in browser JS; (2) polling requires a timer, adds latency, and creates unnecessary load; (3) postMessage is event-driven with zero latency.

**Configurator redirects to a host-provided completion URL (rejected)**
After completion, the configurator redirects the iframe to a URL the host provided at session creation. Rejected because: (1) it requires the host to expose a public completion endpoint; (2) it destroys the iframe session state; (3) it cannot carry the completion payload to the host without URL query parameters, which has length and encoding limits; (4) it breaks if the host does not want to handle a redirect at all.

**SharedWorker or BroadcastChannel (rejected)**
Both require the frames to be on the same origin. Not applicable here.

---

## Consequences

- Host pages must register a `message` event listener and filter by `type` to avoid acting on unrelated messages from browser extensions or other frames.
- The configurator does not need to know whether it is in an iframe; the `try/catch` around `window.parent.postMessage` is enough.
- The message shape is part of the integration contract. Any field added to or removed from the message is a breaking change for host-side listeners.
- `authoritativePrice` in the message comes from the server's completion response, not a client-side estimate. This is consistent with the server-authoritative pricing model (ADR 0006).
- The postMessage listener and the server-side webhook (ADR 0001) serve different consumers. Both run on session completion: the webhook delivers to the CRM backend; the postMessage delivers to the browser host page.
