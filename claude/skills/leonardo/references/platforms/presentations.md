# Presentations: PowerPoint

A presentation is read from a distance, often only once. It has to work for the audience, not for the author.

## Research first

For features you are not sure about, look up the current PowerPoint help (see `sources.md`), for example on "Morph", animation timing, slide masters and the accessibility checker.

## Structure and usability

- One message per slide. The title states the message as a sentence, not a topic word.
- Use the slide master and its layouts. Place nothing by eye outside the grid.
- Every image has alt text; run PowerPoint's accessibility checker before sharing.
- Customers receive the PDF, not the editable file.

## Leonardo in presentations

- **Typeface:** Inter, titles in Inter Display. Never system typefaces.
- **Type roles:** place name (slide title) Inter Display 40 pt Light, section Inter 24 pt Semibold, body Inter 18 pt, meta Inter 12 pt.
- **Colors:** only from the blue scale and the cool neutrals in `leonardo.tokens.json`. One accent surface per slide. Charts use the blue scale; status colors only for real status.
- **Alignment line:** title, body text and charts share one left edge.
- **Logo:** only the files from the logo package, with clear space; the short form below the minimum size.

## Motion

- **Dive in and transform:** the "Morph" transition, 0.5 s. An element that becomes the next title carries the same name on both slides in the selection pane, prefixed with `!!`.
- **Stagger and arrive:** only "Fade", optionally with a short motion path; "With previous" with delays in steps of 0.05 s.
- **Arrive:** smooth end fully applied, 0.3 s. **Leave:** smooth start fully applied, 0.2 s, with fade.
- **Slide transitions:** "Morph", "Fade" or none.
- **Never:** effects that fly in from outside the slide, spin, bounce or jump at the end.
