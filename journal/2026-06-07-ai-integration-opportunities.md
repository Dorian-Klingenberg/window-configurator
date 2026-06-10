# Journal — June 7, 2026

## Session Summary

Dorian asked how AI should fit into RenoNerd / WindowConfigurator and then clarified two especially promising directions:

- voice-driven contractor measure-up capture
- pricing capture from painful authorized manufacturer/dealer configurators

This was documentation-only. No code, tests, branches, remotes, or implementation-roadmap phase transitions were changed.

## What Changed

- Updated `window-domain-knowledge.md` with an `AI-Adjacent Workflow Opportunities` section.
- Updated `README.md` portfolio notes with a concise AI extension summary.

## Product Notes

### Field Measure Voice Capture

This is the strongest customer-facing AI wedge.

Workflow:

1. Contractor visits the customer's house for a measure-up.
2. Contractor walks room to room with a phone and dictates each opening.
3. Speech-to-text plus a constrained parser or LLM turns the transcript into a draft configured item.
4. The server validates dimensions, product-line policy, style compatibility, option availability, pricing-grid support, and order-readiness.
5. The contractor confirms or corrects the draft before moving on.
6. The homeowner can later receive a magic link and review customer-editable choices without changing installation-critical fields.

Useful product sentence:

> Walk the house, speak the windows, leave with a quote.

Important boundary:

> AI proposes draft configuration; deterministic backend services own truth.

### Pricing Capture Harness

This is the stronger engineering/portfolio wedge.

Contractors may have authorized access to slow manufacturer/dealer configurators. A pricing capture harness could generate many valid configurations, drive the external configurator, capture returned prices and warnings, and infer pricing-grid behavior.

The core should be deterministic:

- configuration generation
- browser automation
- output capture
- evidence snapshots
- pricing-grid inference
- anomaly reports

AI can help with ambiguous labels, option mapping, error classification, anomaly summaries, and next-batch suggestions. AI should not own captured prices.

Professional framing:

> Automated price-book reconstruction and validation for dealers using their authorized manufacturer quoting tools.

## Decisions Made

- Do not frame the RenoNerd AI opportunity as a generic chatbot.
- Preserve the current architecture principles:
  - server-authoritative pricing
  - server-side validation
  - exact measurement math
  - CRM-agnostic webhooks
  - no outbound CRM API calls
- Keep homeowner editing constrained after contractor measure-up. Installation-critical fields should stay locked unless the contractor changes them.
- Build transcript-to-validated-draft first if the voice feature starts; live microphone capture can come later.
- Design deterministic pricing evidence capture before adding any AI interpretation layer.

## Validation Performed

- Read:
  - `D:\Repos\renonerd\AGENTS.md`
  - `AGENTS.md`
  - `README.md`
  - `implementation-roadmap.md`
  - `window-domain-knowledge.md`
  - `journal/2026-05-27.md`
  - `adr/0015-mock-demo-host-architecture.md`
  - `adr/0016-iframe-embedding-as-configurator-integration-pattern.md`
  - `adr/0017-postmessage-for-session-completion-signaling.md`
  - `adr/0018-crm-completion-status-via-polling-and-session-api-extension.md`
- Checked `git status --short` before edits; the WindowConfigurator worktree was clean.

## Current State

These AI ideas are now durable project memory, but they are not implemented features and do not change the active phase discipline. If implementation begins, follow TDD, verify current branch/phase status, and keep the server-authoritative boundary intact.

## What Comes Next

- [ ] If voice intake starts, write tests around transcript-to-structured-draft parsing before implementation.
- [ ] Add validation tests showing AI drafts cannot bypass server product/style/pricing rules.
- [ ] Prototype the voice flow as text transcript input before live microphone capture.
- [ ] If pricing capture starts, define the deterministic configuration-generation and evidence artifact contract first.
- [ ] Add an explicit permission/authorization note before any external configurator automation is attempted.
