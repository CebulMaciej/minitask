# Design Tokens — Dark Mode

FitPlan uses a dark-first design with high contrast and energetic accent colors suited for a fitness brand.

## Colors

### Brand
- `primary`: #C8FF00 (electric lime — action color, CTAs, highlights)
- `primary-dark`: #A3D400
- `accent`: #FF6B35 (orange — unexpected progress, achievements)

### Dark Background Scale
- `bg-base`: #0D0D0D (page background)
- `bg-surface`: #1A1A1A (cards, panels)
- `bg-elevated`: #242424 (modals, dropdowns)
- `bg-subtle`: #2E2E2E (input backgrounds, dividers)

### Text
- `text-primary`: #F5F5F5
- `text-secondary`: #A0A0A0
- `text-muted`: #606060
- `text-inverse`: #0D0D0D (text on primary/lime backgrounds)

### Semantic
- `success`: #22C55E
- `warning`: #F59E0B
- `error`: #EF4444
- `info`: #3B82F6
- `progress`: #C8FF00 (unexpected progress highlight — same as primary)

### Border
- `border-default`: #2E2E2E
- `border-subtle`: #1F1F1F

## Typography

- `font-display`: "Bebas Neue", "Impact", sans-serif (headings, numbers, metrics)
- `font-body`: "Inter", system-ui, sans-serif (body text, labels)
- `font-mono`: "JetBrains Mono", monospace (weights, reps counters)

### Scale
- `text-xs`: 0.75rem / 12px
- `text-sm`: 0.875rem / 14px
- `text-base`: 1rem / 16px
- `text-lg`: 1.125rem / 18px
- `text-xl`: 1.25rem / 20px
- `text-2xl`: 1.5rem / 24px
- `text-3xl`: 1.875rem / 30px
- `text-4xl`: 2.25rem / 36px (display numbers — weights, reps in live session)

## Spacing
- `spacing-1`: 0.25rem
- `spacing-2`: 0.5rem
- `spacing-3`: 0.75rem
- `spacing-4`: 1rem
- `spacing-5`: 1.25rem
- `spacing-6`: 1.5rem
- `spacing-8`: 2rem
- `spacing-10`: 2.5rem
- `spacing-12`: 3rem

## Border Radius
- `radius-sm`: 0.25rem
- `radius-md`: 0.5rem
- `radius-lg`: 0.75rem
- `radius-xl`: 1rem
- `radius-full`: 9999px (pill badges)

## Shadows (dark-mode appropriate — glow over drop)
- `glow-primary`: 0 0 12px rgba(200, 255, 0, 0.25) (used on focused inputs, active states)
- `shadow-card`: 0 1px 3px rgba(0, 0, 0, 0.5)
- `shadow-elevated`: 0 8px 24px rgba(0, 0, 0, 0.6)

## Tailwind Config Mapping

These tokens map directly to a custom Tailwind config (`tailwind.config.js`):
- `bg-base` → `bg-fitplan-base`
- `primary` → `text-primary` / `bg-primary` / `border-primary`
- `accent` → `text-accent` / `bg-accent`
