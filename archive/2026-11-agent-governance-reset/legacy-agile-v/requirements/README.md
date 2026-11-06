# Requirements

This directory is the **Phase A entry point**. One file per slice, written before the branch is touched.

---

## The gate

Before any implementation work begins on a slice — before the branch is created, before a test is written — a requirements document for that slice lives here. Writing this document is the act that formally opens the slice. It is the Agile V first component.

The Agile flexibility lives *above* this gate: choosing which slice comes next, deciding what goes into it, deferring or dropping scope. All of that is flexible until the requirements doc is written. Once it is written, the requirements are baselined for the duration of the slice. Changes during implementation go through a decision gate (journal entry + possible ADR), not silent drift.

---

## File naming

```
requirements/slice-N-kebab-topic.md
```

Examples:
- `requirements/slice-d-demo-ux-hardening.md`
- `requirements/slice-e-pricing-server-authority.md`

---

## Requirement format

Each requirement is one entry with a stable ID:

```markdown
## REQ-D-001

**Statement:** The configurator loading state must display a spinner visible within 200ms of iframe mount.

**Rationale:** Users on slow connections currently see a blank iframe with no feedback.

**Acceptance:** A test confirms the spinner element is present in the DOM before the configurator API responds.
```

Rules:
- One sentence per statement — if it takes two sentences, split it into two requirements
- Every requirement must be testable — if you cannot write a failing test for it, it is not a requirement
- Scope to the slice — cross-slice concerns belong in ADRs, not here
- IDs are stable once assigned — never renumber

---

## Current requirements documents

| Slice | File | Status |
|---|---|---|
| Slice D — Demo UX Hardening | *(not yet written — write before starting Slice D branch)* | ⬜ Pending |
