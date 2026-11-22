# Mode: Build

For new screens, features or apps.

## Steps

1. **Understand the task.** Answer the questions in `usability.md` ("Before designing"). If you cannot, ask.
2. **Research.** Read the official guidance for the platform and for every pattern you plan to use (`sources.md`). Note title, URL and date.
3. **Structure first, looks later.** Decide navigation, the main task, commands and their places, and how the view answers "Where am I? What can I do? What just happened?". Write this down in a few lines before writing UI code.
4. **Choose platform components.** Only build something custom when the platform has nothing for it, and then follow the platform's interaction rules.
5. **Apply Leonardo to the content layer.** Type roles, alignment line, grid, density, color semantics, status shapes, motion vocabulary; all values from tokens.
6. **Design every state** in the state matrix of `usability.md`.
7. **Write the text** following the platform's writing guide.
8. **Look at the result** in light and dark mode, with large text and keyboard only, where the environment allows. If you cannot build or run it (no toolchain, no simulator, no display), say so in the pull request and list what is unverified, for example APIs you are unsure of.

## Checklist before the pull request

- [ ] The platform's official guidance was read for every pattern used; sources are listed.
- [ ] Navigation, commands, dialogs and settings follow the platform conventions.
- [ ] Each view answers the three questions.
- [ ] All states of the state matrix are designed.
- [ ] No literal design values; all from tokens or Leonardo resources.
- [ ] Accessibility: names and roles, keyboard, focus, contrast, large text, reduced motion, high contrast.
- [ ] Text follows the platform's writing guide; buttons use specific verbs.
- [ ] Motion uses only the seven Leonardo motions with platform means.

## Pull request description

- What the user can do now, in one or two sentences.
- Sources read (title, URL, date).
- Decisions where platform and Leonardo met, and why.
- Leonardo values whose token is marked `"status": "proposal"` and that became visible, so they can be confirmed.
- States covered, and anything not covered yet.
- Open questions.
