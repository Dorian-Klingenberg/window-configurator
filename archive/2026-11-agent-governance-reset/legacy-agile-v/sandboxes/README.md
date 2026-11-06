# Sandboxes

This directory is the **Pre-Phase A zone**. V-Model discipline does not apply here.

No requirements doc. No traceability matrix. No formal verification pass. The only rule is that work here must not pollute the main project's source, tests, or build.

---

## What belongs here

Any bounded experiment whose primary goal is learning rather than shipping:

- A single HTML page with inline Blazor or vanilla JS to test a UI pattern
- A throwaway .NET console project to verify how an OIDC handshake works
- A visualization dashboard to explore data before deciding what to measure
- A pure-markdown design exploration with no code at all

If you're not sure whether something belongs here or in a production slice, ask: "am I trying to learn something, or am I trying to ship something?" If learning, it belongs here.

---

## Naming convention

```
sandboxes/NNN-kebab-question/
```

Examples:
- `sandboxes/001-oidc-handshake-spike/`
- `sandboxes/002-blazor-wasm-inline-configurator/`
- `sandboxes/003-metrics-dashboard-prototype/`

Number sequentially. The name should describe the question being asked, not the technology being used.

---

## Required structure per sandbox

```
sandboxes/NNN-kebab-question/
├── README.md     ← the question, written BEFORE any code
└── (whatever code the question requires)
```

### The sandbox README

Write the README before writing any code. It must answer:

1. **Question** — what are you trying to learn?
2. **Hypothesis** — what do you expect to find?
3. **Time limit** — how long will you spend before declaring a result?
4. **Definition of done** — what outcome closes this sandbox?

The README is the evidence that this was deliberate exploration, not abandoned work.

---

## The exit artifact: a lesson

Every sandbox that produces a result — positive, negative, or inconclusive — exits with a lesson in `lessons/`. The lesson must include a chapter titled **"What This Tells the Project"** that explicitly states the connection (or absence of one) back to the main codebase. This chapter can legitimately say:

- "This approach works. Here is the shape it would take in a production slice."
- "This approach doesn't work because X. We won't pursue it."
- "This is promising but needs more investigation. Next sandbox: NNN."
- "This is interesting but not relevant to the current project direction."

A sandbox does not have to produce a requirement. Understanding is a valid result.

Register the lesson in `lessons/LESSON_CATALOG.md` under a **Sandboxes** section.

---

## What this zone is not

- Not a dumping ground for abandoned feature work
- Not a place to park "maybe someday" code indefinitely
- Not exempt from the lesson requirement — if you close a sandbox, write the lesson

If a sandbox has been sitting for more than one sprint with no lesson and no clear next step, it is stale and should be closed with a one-paragraph lesson explaining why it was abandoned.
