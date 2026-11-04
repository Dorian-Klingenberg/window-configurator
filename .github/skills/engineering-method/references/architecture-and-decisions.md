# Architecture and decisions

Consider the work's actual stage: discovery of a problem and stakeholders; clarification of needs and constraints; exploration of design; implementation; or verification and validation. These are lenses, not mandatory sequential gates. Evidence from a later activity can cause an earlier assumption to be revisited.

Before proposing a structural change, inspect the relevant existing code, documentation, responsibilities, and dependencies. Explain what is observed, what is inferred, and how the proposed change affects existing boundaries. Prefer the smallest design that addresses the actual problem and project conventions. Discuss meaningful architectural choices with the user before acting beyond the approved plan.

A compact architecture map or diagram can help people and agents carry forward decisions, but it is not proof that the implementation conforms. Treat diagrams as representations to validate and maintain when useful, not mandatory artifacts.

Preserve history and useful lessons. Distinguish exploratory ideas from agreed decisions and baselines. If a new decision supersedes an old one, explain the change and its consequences rather than silently rewriting the rationale.

The user has explored whether more constrained tasks can be handled reliably by smaller or less autonomous models. Treat that as an empirical hypothesis, not a universal rule. Select or recommend capabilities according to the remaining decision authority, risk, and evidence; do not create or orchestrate additional agents without a request.
