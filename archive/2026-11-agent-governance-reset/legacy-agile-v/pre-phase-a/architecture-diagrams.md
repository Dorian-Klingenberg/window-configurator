# System Architecture Diagrams — WindowConfigurator

*Pre-Phase A artifact · Version 1.0 · 2026-06-12*

---

## 1. System Context

WindowConfigurator sits between contractors and their customers. It does not call CRMs; it receives calls from them and fires events back.

```mermaid
C4Context
    title WindowConfigurator — System Context

    Person(contractor, "Window Contractor", "Captures windows on-site, issues customer links, confirms remeasures, signs orders")
    Person(customer, "Customer / Homeowner", "Reviews configuration, selects aesthetics, communicates preferences")

    System(wc, "WindowConfigurator", "Two-sided window replacement platform: on-site capture, customer self-service, measurement confidence model, order readiness")

    System_Ext(crm, "Contractor CRM", "Job tracking, customer records, sales pipeline")
    System_Ext(manufacturer, "Window Manufacturer", "Receives completed order configurations")
    System_Ext(image_ai, "Image Processing Service", "Derives rough opening estimate from exterior photo")
    System_Ext(speech_ai, "Speech Recognition Service", "Parses spoken specification into structured fields")

    Rel(contractor, wc, "Captures windows, reviews sessions, enters remeasure, signs off")
    Rel(customer, wc, "Reviews configuration, edits aesthetics, views pricing")
    Rel(crm, wc, "Launches contractor-initiated sessions via API")
    Rel(wc, crm, "Fires outbound webhooks on state transitions")
    Rel(wc, image_ai, "Sends exterior photo, receives rough opening estimate")
    Rel(wc, speech_ai, "Sends audio, receives parsed specification")
    Rel(contractor, manufacturer, "Places order (outside system boundary)")
```

---

## 2. Two Entry Points — Flow Comparison

```mermaid
flowchart TD
    subgraph A["Entry Point A — Contractor-Led"]
        A1[Contractor on-site] --> A2[Photo + Voice capture]
        A2 --> A3[Image estimate + Speech parse]
        A3 --> A4[Contractor reviews & confirms]
        A4 --> A5[Session created — ImageEstimate tier]
        A5 --> A6[Customer link sent]
    end

    subgraph B["Entry Point B — Customer-Led"]
        B1[Customer finds contractor online] --> B2[Customer self-configures]
        B2 --> B3[Optional: customer enters own sizing]
        B3 --> B4[Session created — CustomerEntered tier]
        B4 --> B5[Contractor notified via webhook]
        B5 --> B6[Contractor schedules visit]
        B6 --> A2
    end

    A6 --> C[Customer reviews, edits aesthetics]
    C --> D[Customer communicates preference to contractor]
    D --> E[Contractor physical remeasure visit]
    E --> F[Interior frame + brick mold measured]
    F --> G[Session → PhysicalConfirmed tier]
    G --> H[Contractor signs off]
    H --> I[Order-ready configuration produced]
```

---

## 3. Measurement Confidence State Machine

```mermaid
stateDiagram-v2
    [*] --> CustomerEntered : Customer self-configures\nor enters own sizing
    [*] --> ImageEstimate : Contractor on-site capture\n(photo processed)
    CustomerEntered --> ImageEstimate : Contractor visits,\ntakes photo
    ImageEstimate --> PhysicalConfirmed : Contractor pulls casing,\nmeasures interior frame\n+ brick mold
    CustomerEntered --> PhysicalConfirmed : Contractor skips image estimate,\ngoes straight to remeasure

    PhysicalConfirmed --> [*] : Order placed

    note right of CustomerEntered
        Pricing label: "Indicative estimate"
        Order-ready: No
        Customer can modify sizing: Yes
    end note

    note right of ImageEstimate
        Pricing label: "Estimate — based on site photo"
        Order-ready: No
        Customer can modify sizing: No (contractor-locked)
    end note

    note right of PhysicalConfirmed
        Pricing label: "Confirmed price"
        Order-ready: Yes
        Customer can modify sizing: No
    end note
```

---

## 4. Component Architecture

Extends ADRs 0001–0018. New components introduced by the evolved product direction are marked **[NEW]**.

```mermaid
flowchart TB
    subgraph Client["Client — Blazor WebAssembly [NEW: Blazor]"]
        CP[Contractor Portal\nMobile-first capture UI]
        CS[Customer Session\nAesthetics + live pricing]
        PE[Pricing Engine\nC# running in-browser]
    end

    subgraph Server["Server — ASP.NET Core"]
        API[Session API\nPOST/GET/PUT sessions]
        PS[Pricing Service\nAuthoritative calculation]
        VS[Validation Service\nCompletion rules]
        CR[Catalog Resolver\nProduct line + assets]
        WD[Webhook Dispatcher\nOutbound events + retry]
        RM[Remeasure Controller\n[NEW] Physical measure entry]
        SL[Session Link Generator\n[NEW] Signed token URLs]
    end

    subgraph Pipelines["AI Pipelines [NEW]"]
        IP[Image Processing\nExterior photo → rough opening estimate]
        SR[Speech Recognition\nAudio → specification fields]
    end

    subgraph Data["Data"]
        DB[(SQL Database\nSessions, items, tenants,\nmeasurement source)]
        PG[(Pricing Grid\nSerialized at session open)]
    end

    subgraph External["External"]
        CRM[Contractor CRM]
        MFG[Manufacturer]
    end

    CP --> IP
    CP --> SR
    IP --> API
    SR --> API
    CS --> PE
    PE --> PG
    API --> PS
    API --> VS
    API --> CR
    API --> WD
    API --> RM
    API --> SL
    PS --> DB
    CR --> DB
    WD --> CRM
    RM --> DB
    SL --> CS
```

---

## 5. On-Site Capture Sequence

```mermaid
sequenceDiagram
    participant C as Contractor (mobile)
    participant CP as Contractor Portal
    participant IP as Image Processing
    participant SR as Speech Recognition
    participant API as Session API
    participant DB as Database

    C->>CP: Photograph window (exterior)
    CP->>IP: POST exterior photo
    IP-->>CP: Estimated rough opening (fractional)

    C->>CP: Speak specification\n("right casement, two-over-two, white")
    CP->>SR: POST audio
    SR-->>CP: Parsed fields (type, operation, grille, finish)

    CP->>C: Review screen: estimated dims + parsed spec
    C->>CP: Confirm (or correct fields)

    CP->>API: POST session with ImageEstimate source
    API->>DB: Persist session, items, measurement source
    API-->>CP: Session ID + customer link token
    CP->>C: Customer link ready to send
```

---

## 6. Architectural Decisions Inherited

The following ADRs govern the backend contract and remain in force for the evolved system:

| ADR | Decision | Still applies |
|---|---|---|
| 0001 | Unidirectional webhook integration — no CRM API calls | Yes — non-negotiable |
| 0002 | Two-flow architecture (CRM-launched, website-launched) | Yes — extended to contractor-led / customer-led |
| 0003 | Session-centric domain model | Yes |
| 0004 | Product line as session input | Yes |
| 0005 | Completion payload contract | Yes |
| 0006 | Runtime pricing grid alignment | Yes — grid now also preloaded to client |
| 0007 | Minimal versioned session API surface | Yes |
| 0008–0009 | Webhook dispatch and durable delivery tracking | Yes |
| 0010–0014 | Multi-tenant hardening, API key, retry, E2E harness | Yes |
| 0015–0018 | Mock demo host architecture, iframe, postMessage, CRM polling | Yes — demo surfaces remain |

**New architectural decisions required (to be written as ADRs when implementation begins):**
- ADR-NEW-A: Image processing service selection and integration pattern
- ADR-NEW-B: Speech recognition service selection and integration pattern
- ADR-NEW-C: Blazor WebAssembly adoption — migration strategy from Knockout.js
- ADR-NEW-D: Pricing grid serialization format for client-side preload
- ADR-NEW-E: Measurement source state machine and transition rules
- ADR-NEW-F: Session link token format, signing, and expiry
