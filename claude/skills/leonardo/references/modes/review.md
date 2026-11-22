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

## Fix first
The three to five changes with the most effect, in order. Group findings with one root cause into one item
(for example "every control is a div" covers keyboard, focus and screen reader findings) and name their IDs.

## Findings
| ID | Severity | Where | Problem | Why it matters | Recommendation | Source |
| --- | --- | --- | --- | --- | --- | --- |
| R1 | 3 | Customers › Edit | … | Heuristic 5, error prevention | … | HIG: … (URL) |

## What works well

## Limitations
What could not be checked and how the review was done, for example "read from the source, the app was not run",
"no screen reader test", or a source that could not be fetched.

## Open questions
```

## Rules for findings

- One problem per finding; describe what you observed, not a guess about intent.
- Every finding names its basis: an accessibility criterion (for example "WCAG 2.2 SC 2.4.7"), a heuristic (for example "Heuristic 5"), a platform guideline with URL, or a Leonardo rule with its section and level (for example "Leonardo, Color as signal, MUST").
- A finding with several bases lists them all; the strongest decides the severity.
- Recommendations are concrete enough to implement, and respect the order of priority.
- Leonardo deviations without usability impact are severity 1 at most.

## Checklist before handing over the report

- [ ] Scope, main tasks and the sources read (title, URL, date) are named.
- [ ] Every finding has one problem, a severity, a basis and a concrete recommendation.
- [ ] Accessibility failures against WCAG A or AA are rated 3 or 4.
- [ ] "Fix first" names the changes with the most effect.
- [ ] Limitations say what was not checked.
- [ ] No code was changed, unless the user asked for it.
