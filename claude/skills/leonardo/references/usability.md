# Usability

These rules apply on every platform. They are interpreted through the platform's conventions: what counts as "standard" is what the platform's own apps do. The binding rules are in the section "Usability" of `design-language.md`; this file adds how to apply and check them.

## Before designing: understand the task

Answer these before you draw anything. If you cannot, ask.

1. Who uses this view, and how often: daily for hours, or once a month?
2. What are they trying to achieve here? Name the one or two main tasks.
3. What do they need to see to decide, and what do they need to do to finish?
4. What happens before and after this view?

Design for the main task first. Everything else is secondary and may move into menus, context menus or details.

## Three questions every view answers

| Question | Typical answer |
| --- | --- |
| Where am I? | Place name, selection in the navigation, window title |
| What can I do here? | Visible primary commands, context menus, empty-state actions |
| What just happened? | Status near the object, summary line, short confirmation, undo |

## State matrix

Design and check every view in these states:

| State | What to check |
| --- | --- |
| Empty | Explains what will appear, offers the first action |
| Loading | Shows structure or progress; long waits can be cancelled; nothing else blocks |
| Partial data | Works with missing optional data; no "null", no broken layout |
| Error | Says what happened, why, what to do; keeps the user's input |
| Offline or no permission | Explains the situation and what still works |
| Long and translated text | German and long names wrap instead of clipping |
| Large text | Platform text scaling works without overlap |
| Dark mode and high contrast | Every element visible; status still recognizable without color |
| Reduced motion and no transparency | Same meaning, no motion, opaque surfaces |
| Keyboard only | Every command reachable; focus always visible; logical order |
| Screen reader | Names, roles and states announced; live changes announced |

## Heuristics for reviews

Use the ten usability heuristics of Nielsen Norman Group as the frame for findings:

1. Visibility of system status
2. Match between the system and the real world
3. User control and freedom
4. Consistency and standards (on our side: platform guidelines and Leonardo)
5. Error prevention
6. Recognition rather than recall
7. Flexibility and efficiency of use
8. Aesthetic and minimalist design
9. Help users recognize, diagnose and recover from errors
10. Help and documentation

## Severity

Rate every finding:

| Rating | Meaning |
| --- | --- |
| 4 – Catastrophe | Users cannot complete a task, lose data, or are excluded (accessibility). Fix before release. |
| 3 – Major | Users struggle or make errors often. Fix with high priority. |
| 2 – Minor | Users are slowed down or irritated. Fix when touching the area. |
| 1 – Cosmetic | Only noticed on close inspection, for example a Leonardo deviation without usability impact. |
| 0 – No problem | Noted for completeness. |

Accessibility failures against WCAG level A or AA are always at least 3.

## Writing

- Buttons: a verb that describes the result, such as "Save changes", "Delete 3 files", "Send invitation".
- Errors: what happened, why, what to do. No codes without explanation. No blame.
- Capitalization, punctuation and terms follow the platform's writing guide. Microsoft and Apple differ; look it up in `sources.md`.
- Use the user's words, not internal or technical ones.
