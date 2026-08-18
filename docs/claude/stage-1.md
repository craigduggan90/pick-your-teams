# Stage 1 — Foundations

## Context

`claude.md` defines a five-stage build order for the Pick Your Teams frontend, built from
scratch (`src/` currently only contains the .NET API — no frontend code exists yet). Stage 1
("Foundations") is explicitly scoped to tooling, theming, and a set of tested primitive
components — no real screens. The goal is to make every architectural decision that later
stages would otherwise have to revisit (styling system, component library, test setup, CI,
folder boundaries between visual/non-visual code) so Stages 2–5 can focus purely on screens.

Resolved via design review before this stage started (see `claude.md`'s Decisions log for the
full set; the two relevant to this stage's scope):
- Floating labels are a required TextInput behavior (not diagram flourish).
- The Stage 1 component showcase page (see Verification) is kept permanently as a
  `/dev/components` route, not deleted after manual QA — it doubles as an ongoing visual
  reference as more primitives are added in later stages.

## Approach

### 1. Branch
`stage-1-foundations`, branched off the latest `main`, per the Workflow section of `claude.md`.

### 2. Scaffold
Vite + React + TypeScript app in `src/ui/`. Self-contained with its own `package.json`,
mirroring how `src/api` is self-contained under its own directory with its own CI
`working-directory`. Node pinned via `.nvmrc` + an `engines` field.

### 3. Styling — Tailwind v4
Installed via the `@tailwindcss/vite` plugin (no separate PostCSS config needed). The semantic
palette from `claude.md`'s Design Tokens section is defined as CSS custom properties in an
`@theme` block in `src/ui/src/index.css`:

`--color-primary`, `--color-secondary`, `--color-tertiary`, `--color-success`,
`--color-warning`, `--color-error`, `--color-info`, `--color-dark-grey` (body text),
`--color-light-grey` (disabled/placeholder).

The diagrams only label these as "Colour Placeholder" (no real hex values), so the chosen hex
values are sensible, distinguishable placeholders, documented as swappable — not final brand
colors. System font stack (no custom font loading), mobile-first layout with a centered
fixed-max-width column at desktop breakpoints.

### 4. Component library — shadcn/ui
`shadcn` initialized against the Tailwind v4 setup, with the unstyled Radix primitives needed:
`button`, `input`, `dialog` (→ Modal), `select`. Toast uses `sonner` (shadcn's current
recommended replacement for the deprecated `toast` component) rather than the old primitive.

### 5. Folder structure (`src/ui/src/`)
Matches the Stack section's separation of non-visual and visual code:
- `components/` — our primitives (Button, TextInput, Toast, Modal, Select, Header, Footer), each
  with a colocated `.test.tsx`
- `components/ui/` — raw shadcn-generated vendor primitives, left unmodified
- `api/`, `hooks/`, `lib/` — empty scaffolded folders ready for Stage 2+, establishing the
  boundary now rather than retrofitting it
- `pages/` — empty for now (Stage 2+ adds real screens); holds the `/dev/components` showcase
  page
- `App.tsx` — wires `QueryClientProvider` (TanStack Query) and `BrowserRouter` (react-router)
  with no real routes yet, so Stage 2 can add routes without restructuring the shell

### 6. Primitives built
- **Button** — variants for primary/secondary actions and a destructive variant (for
  "Remove"/"Delete" actions called out in the Stack section's shadcn rationale)
- **TextInput** — wraps shadcn `Input`; floating-label behavior (label animates on
  focus/has-value) and an error-state prop (red border + inline message)
- **Select** — wraps shadcn `Select`; options passed generically as props (current-state-aware
  option logic per player is Stage 4's concern, not this stage's)
- **Modal** — wraps shadcn `Dialog`; generic shell (title, body slot, footer actions) so the
  "Remove @Tag?" / Delete Account / Record Result reuse pattern in `claude.md` works without new
  visual design in later stages
- **Toast** — thin wrapper around `sonner` exposing `toast.success`/`toast.error` helpers mapped
  to the Success/Error palette tokens
- **Header/Footer shell** — Header uses the Primary color; Footer carries the small app-name
  text noted on `02-games-list.png`. No nav logic yet (no routes exist to link to)

### 7. Testing
Vitest + jsdom + React Testing Library + `@testing-library/jest-dom`, with a
`src/test/setup.ts` for jest-dom matchers. Each primitive has a colocated test covering render +
key interactive behavior (TextInput error message, Modal open/close callback, Select onChange,
Toast triggers) — no snapshot tests.

Playwright is **not** set up this stage — there are no real screens yet to E2E test.

### 8. CI
`.github/workflows/ui-build-and-test.yml`, mirroring the existing `build-and-test.yml`'s style
(emoji step names, `working-directory: src/ui`, triggered on push): checkout →
`actions/setup-node@v4` → `npm ci` → `npm run build` (typecheck + Vite build) →
`npm run test -- --run` (Vitest). This is new — no CI previously covered anything under
`src/ui`, and the Workflow section of `claude.md` expects CI to run against the PR.

### 9. `.gitignore`
`dist/` added at the root — was missing, needed for the Vite build output, doesn't conflict
with the existing .NET-focused ignore rules.

## Explicitly out of scope for this stage
- No real routes/pages, no auth wiring (Stage 2), no API calls (Stage 2+)
- No Playwright setup (nothing to E2E test yet)
- No ESLint/Prettier beyond whatever Vite's template scaffolds by default

## Verification
- `npm run build` succeeds (typecheck + Vite build) in `src/ui`
- `npm run test -- --run` passes all primitive component tests
- `npm run dev`, checked in a real browser: the permanent `/dev/components` showcase page
  renders every primitive (Button variants, TextInput with floating label + error state, Modal
  open/close, Select, Toast trigger) plus the Header/Footer shell
- The new GitHub Actions workflow runs green once the draft PR is pushed
