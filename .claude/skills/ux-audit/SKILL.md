---
name: ux-audit
description: Audit one UI surface against interaction patterns and UX checklists using static code review, rendered screenshots, and dynamic user journeys.
argument-hint: "<page-or-surface> [--l1 | --l2 | --l3]"
---

# UX Audit

A repeatable, evidence-based review of one UI surface at a time.

## Benchmark
Use both:
1. Jenifer Tidwell's *Designing Interfaces* pattern language.
2. The product's UX checklists and domain conventions.

Audit one surface per run. Re-run as the product evolves.

## Three layers

| Layer | Method | Finds | Cost |
|---|---|---|---|
| L1 Static | Read code | Missing states/branches, absent patterns, structural issues | Low |
| L2 Visual | Inspect rendered screenshots | Visual hierarchy, spacing, contrast, alignment, truncation, overflow, responsive behavior | Medium; needs a render |
| L3 Dynamic | Drive a real user journey | Locked/in-progress states, errors, focus/keyboard, journey continuity, performance measurements | High; needs a running environment |

### Coverage rules
- L1 is the baseline for every audit.
- Add L2 for layout, hierarchy, rendered states, or responsive behavior.
- Add L3 for journeys, forced states, keyboard behavior, or performance measurement.
- Do not claim visual correctness from code alone.
- Do not claim runtime behavior without running the application.

## Ground rules
- Every finding must cite evidence: file and line for L1, verified screenshot for L2, captured value or snapshot for L3.
- Benchmark the surface class, not just the implementation. Identify expected capabilities from comparable products before concluding that a screen is complete.
- When comparing variants, define the success metric first. Do not declare a behavioral winner from static code or screenshots alone; use L3/analytics.
- Treat apparently redundant controls as a composition question before deleting one; controls may have different scopes.
- Report strengths as well as gaps. Identify behaviors that should not regress.
- Rank issues by severity:
  - Critical: breaks trust, loses data/input, or creates a permanent stuck state.
  - High: dead-end, misleading state, missing forward path or feedback.
  - Medium: friction, inconsistency, weak hierarchy, or missed usability improvement.

## Output format
1. Layers run and evidence inspected.
2. Patterns in use, grouped by pattern family.
3. Strengths / good cases to preserve.
4. Ranked experience gaps, each with evidence, violated pattern/checklist, and a concise remedy.
5. Skill feedback: existing rules validated, generalizable gaps, and good examples.
6. Fix concrete bugs where authorized; document anything left open and why.
7. Save the audit report as a reusable example for future passes.

## Codebound menu audit checklist
- Is gameplay HUD absent from the main menu?
- Is the main action visually obvious?
- Are all menu actions real and correctly wired?
- Are empty-save and offline states clear?
- Are hover, focus, pressed, disabled, and loading states visible?
- Is keyboard navigation usable?
- Does the layout work at 1080p, 1440p, and 4K?
- Is the visual factory scene readable without competing with navigation?
- Are accessibility and text contrast adequate?

## Important scope note
This skill's original workflow references supporting files (`references/pattern-catalog.md`, layer procedures, examples, and a related `ux` skill). This portable bundle includes the core SKILL.md only; install the upstream package for its full supporting library.
