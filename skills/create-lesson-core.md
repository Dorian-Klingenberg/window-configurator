# Skill: create-lesson (Core Instructions)

This file is the single source of truth for the `create-lesson` skill. Runtime-specific wrappers (Claude Code, Copilot) reference this file. Do not embed runtime-specific syntax here.

---

## Purpose

Produce a structured lesson document from recently completed work — a phase, a slice, or a discrete implementation step. The lesson captures what was built, why, and how, in a format that can be used as a teaching script, video outline, or onboarding reference.

---

## When To Invoke

After completing any phase, slice, or step where:
- New files were created or existing files were significantly changed
- At least one ADR or design decision was made
- Tests were written

---

## What To Read First

Before writing anything, read the following in order:

1. The most recent journal entry (or the journal entry matching the phase/step name provided)
2. Each new or changed source file mentioned in that journal entry
3. The test file(s) for the new code
4. The ADR(s) written for this work, if any
5. The existing `lessons/README.md` to find the next available phase number

Do not make up code snippets. Every snippet in the lesson must come from the actual source files you read.

---

## Lesson File Location and Name

Place the lesson file in `lessons/` using the naming convention:

```
lessons/phase-{N}-{kebab-case-title}.md
```

If a lesson file for this phase already exists, update it rather than creating a duplicate.

---

## Required Structure

Every lesson must contain all of the following sections in this order.

### 1. Title

```
# Phase N Lesson: [Human-readable title]
```

### 2. Why This Phase Exists

One paragraph. Answer:
- What gap existed before this work?
- What risk does this work reduce?
- Why does this come at this point in the sequence (not earlier, not later)?

### 3. One Section Per Slice or Feature

If the work had multiple slices, repeat this block for each one. If it was a single unit of work, one block is fine.

Each block must contain:

**The Gap We Closed**
One or two sentences: what was incomplete or unsafe before this slice.

**What We Built**
A bullet list of new or significantly changed files and classes. Include the file path and one-line description of its role.

**Build Steps**
A numbered list in TDD order:
1. Write the failing test (red)
2. Implement the minimum code to pass (green)
3. Any wiring steps (DI registration, migration, etc.)

**[Descriptive Name] Diagram**
A Mermaid diagram appropriate to the work:
- `sequenceDiagram` for request/response flows
- `flowchart TD` for decision trees or state machines
- Use real class/method names from the code

**Representative Snippet**
One code block showing the key pattern introduced. Use the actual code from the source files. Add a one-line comment explaining what makes this interesting — not what it does (the code shows that), but why this specific approach was chosen.

**Tests Added**
A markdown table:

| Test | Asserts |
|---|---|
| `TestMethodName` | What behavior it proves |

**ADR**
If an ADR was written, include:
```
`adr/XXXX-name.md`
```

---

### 4. What To Teach In A Video

A bullet list of 3–6 concepts from this work that are worth teaching explicitly — things a reader might not infer from just reading the code. Focus on:
- Non-obvious design decisions
- Patterns with broad applicability beyond this project
- "Why not the simpler thing" moments

---

## After Writing the Lesson

1. Open `lessons/README.md` and add an entry for the new lesson in the numbered list, keeping the list in phase order.
2. Confirm the lesson filename matches the `lessons/README.md` link.

---

## Rules

- Do not read or reference files outside `D:\Repos\renonerd\`
- Do not invent code snippets — every snippet must be read from an actual file
- Do not skip the Mermaid diagram
- Do not skip the Tests Added table
- Do not create a new lesson file if one already exists for this phase — update the existing one
- Do not add commentary about the lesson-writing process inside the lesson itself
- One lesson file per phase — multiple slices go in the same file as separate sections

---

## Acceptance Checks

- Can a developer follow the Build Steps section to recreate the work from scratch?
- Does the Mermaid diagram use actual class and method names from the code?
- Is every code snippet present verbatim in a source file in the repo?
- Does `lessons/README.md` have an entry pointing to this file?
- Would the "What To Teach In A Video" section work as a standalone script outline?
