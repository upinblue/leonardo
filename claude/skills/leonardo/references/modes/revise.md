# Mode: Revise

For changing, modernizing or fixing existing UI. Existing users have habits; protect them.

## Steps

1. **Look before you change.** Do a short review of the affected area (see `review.md`), at least the top problems with their severity. Share it before large changes.
2. **Research** the platform guidance for the patterns involved.
3. **Agree on the scope.** Large changes, such as moving commands, renaming things or changing navigation, need the user's confirmation, because they change what people have learned. If the request already sets the scope, say which changes it covers, record the short review in the pull request, and go on.
4. **Change in this order, in separate commits:**
   1. Accessibility problems
   2. Usability problems by severity
   3. Platform conventions: replace custom controls with platform controls
   4. Leonardo: literal values → tokens (status colors → the platform's), then Leonardo resources and controls, then layout (page margin and alignment line, type roles, density), then motion
5. **Keep behavior stable** unless a change was agreed. Keyboard shortcuts, command names and positions stay where users expect them.
6. **Check** with the checklist in `build.md`.

## Pull request description

- Before and after, with screenshots in light and dark mode where possible.
- Every change in behavior, listed explicitly, so it can go into release notes.
- Sources read.
- Findings deliberately left for later, with their severity.
