# Dorian Work and GH-600 Migration Plan

## Starting point

The former governance stack tried to enforce every useful practice globally:
mandatory reading, tool preferences, phase gates, TDD, documentation, branch
rules, and role behavior. That made discovery and learning carry the same
burden as production implementation, while the live project state drifted from
the documents meant to govern it.

The reset retains the product's durable decisions and the two installed agent
bundles. It retires the old instruction stack so that practices can be learned,
used, and kept based on evidence rather than inherited as blanket rules.

## Operating model

| Work mode | Primary agent | Expected output | Change authority |
|---|---|---|---|
| Discovery and design | Ada | Shared understanding, options, decision, or handoff | No repository change until explicitly approved |
| Certification learning | Certification Learning Coach | Conversation, practice, and bounded training records | Only within `training/`, as the agent defines |
| Production implementation | A separately chosen implementation agent | Working change and proportionate verification | Explicitly approved scope |
| Independent verification | A separate review or test-design context | Requirement-based evidence and findings | No implementation changes unless authorized |

The agents define their own behavior. Repository practices should be added only
when a real project need remains after the role contract is considered.

## Reintroduction over the next few months

### 1. Establish a reliable current-state baseline

Before opening the next production slice, use Ada to reconcile the roadmap,
open issues, requirements, and historical handoffs. The outcome is a concise
handoff that states what is current, what is historical, and which decision is
needed next. Do not recreate a universal required-reading list.

### 2. Practice a single requirements-to-verification slice

For the next approved slice, create a scoped requirements baseline and the
Gherkin/scenario artifact requested by issue #7. Use an independent context to
design tests from those requirements, then implement and close with an ATM.
This tests whether the Agile V practices improve clarity and evidence for this
project before they become standing repository policy.

### 3. Capture learning, not ceremony

Use the Certification Learning Coach to connect real GitHub workflow questions
to GH-600 study. Keep any learning records inside `training/`. When a practice
proves useful in production work, record its rationale and limits; do not turn
one successful experiment into a global rule automatically.

### 4. Promote only demonstrated practices

After one or more slices, evaluate the evidence: review quality, traceability,
test usefulness, rework, issue/PR hygiene, and the effort required. Promote a
practice into a small repository guide only when it has a clear purpose, owner,
trigger, and observed benefit. Otherwise keep it as an optional method.

## Guardrails that remain product decisions

The archive does not repeal product architecture. Existing ADRs and
requirements still record decisions such as CRM-agnostic outbound webhooks,
server authority, and exact fractional measurements. Those constraints should
be consulted when a task touches their area; they are not universal agent
bootstrap instructions.

## Success criteria

The migration is successful when:

1. A new agent sees only its own retained bundle, rather than contradictory
   root-level instruction files.
2. Ada can complete discovery and handoff work without being forced into an
   implementation checklist.
3. The learning coach can support certification study without changing product
   code or project governance.
4. The next production slice produces reviewable requirements, scenarios,
   verification evidence, and an honest closeout.
5. The repository's current state is recoverable from live GitHub state and a
   current handoff, while this archive preserves prior context.
