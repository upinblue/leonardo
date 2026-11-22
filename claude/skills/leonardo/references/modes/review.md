# Mode: Review

For evaluating UI and recommending improvements. Do not change code unless the user asks.

## Method

1. **Scope.** Which app, which platform, which views, which main tasks? Ask if unclear.
2. **Research** the platform guidance for the patterns in scope.
3. **Walk through the main tasks** as a user would: from the starting point to the result. Use the running app and screenshots where possible; otherwise read the UI code carefully.
4. **Evaluate in the order of priority:**
   1. Accessibility (WCAG 2.2 AA, platform accessibility guidance)
   2. Usability (the ten heuristics and the state matrix in `usability.md`)
   3. Platform conventions (official guidelines)
   4. Leonardo (`design-language.md`)
5. **Rate** every finding with the severity scale in `usability.md`.
6. **Recommend** concrete changes, each with the reason and the source.
7. **Name what works well.** It tells the team what to keep.

## Report

Write the report as Markdown, by default to `docs/reviews/YYYY-MM-DD-<area>.md`, or in the chat if the user prefers.

```markdown
# Review: <area> (<platform>)

Date, reviewer, scope, main tasks walked through, sources read.

## Summary
Three to five sentences. The most important problems first.

## Findings
| ID | Severity | Where | Problem | Why it matters | Recommendation | Source |
| --- | --- | --- | --- | --- | --- | --- |
| R1 | 3 | Customers › Edit | … | Heuristic 5, error prevention | … | HIG: … (URL) |

## What works well

## Open questions
```

## Rules for findings

- One problem per finding; describe what you observed, not a guess about intent.
- Every finding names its basis: an accessibility criterion, a heuristic, a platform guideline with URL, or a Leonardo rule.
- Recommendations are concrete enough to implement, and respect the order of priority.
- Leonardo deviations without usability impact are severity 1 at most.
