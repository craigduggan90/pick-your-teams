# Stage 3 — Games List, Game View, and My Account

## Context

Stage 2 (branch `stage-2-auth-tag-setup`, merged to `main`) delivered auth, the tag-setup/change-
tag flow, and a `MyAccountPage` placeholder just real enough to link to `/change-tag` from. Stage
3 is `claude.md`'s next step: the Games list (Home screen), the View Game screen, and the real My
Account screen.

Before branching, a prep pass read `claude.md`, `docs/claude/stage-1.md`/`stage-2.md`, and the
`02-games-list.png`/`04-view-game.png`/`03-my-account.png` diagrams, then surveyed the actual
Games API in `src/api` against them. That surfaced real diagram-vs-API conflicts, resolved before
any code was written (see the Decisions log's first entry) — catching these up front avoided
building the Record Result modal or status badges against the wrong values.

## Approach

### 1. Branch

`stage-3-games-my-account`, branched off the latest `main` (which already included the game
organiser info added in `f8cb86a`/`8e22d93` — see Decisions log).

### 2. API layer (`src/ui/src/api/`)

`games.ts` — `GameModel`/`GameDetailModel`/`GameOrganiserModel` types (camelCase, matching the
real wire format confirmed via `src/api` source, not assumed), `GamesPage` as
`{ data, cursor, count }`, and `getGames`/`getGameById`/`createGame`/`updateGame`/`deleteGame`/
`recordResult`. Query params for `getGames` are built PascalCase (matching
`GetGamesRequestModel`'s bound property names directly, same convention as request bodies).
`users.ts` gained `deleteUser`.

### 3. Hooks (`src/ui/src/hooks/`)

`useGames` (TanStack `useInfiniteQuery`, cursor-paginated), `useGame`, `useUpdateGame`,
`useDeleteGame`, `useRecordResult`, `useCreateGame`, `useDeleteAccount`, `useUpdateProfile`. Each
mutation invalidates the relevant query keys (`['game', id]`, `['games']`, `['self']`) on success,
matching Stage 2's `useUpdateTag` pattern.

### 4. Page-actions footer slot (`hooks/usePageActions.tsx`)

New infrastructure, not in the original plan — every screen with buttons "stuck to the bottom" in
the diagrams (Save/Back, New Game/Search, Cancel/Apply) needed a consistent place to put them.
Mirrors the existing `usePageTitle`/`useHeaderTitle` pattern: `usePageFooterActions(node)` plants
content in the shared `Footer`'s action-bar slot, `useFooterActions()` reads it. Two differences
from `usePageTitle`, both discovered the hard way (see Decisions log):

- Split into **two contexts** (state and dispatch), not one — bundling them the way `usePageTitle`
  does caused a real infinite render loop.
- **Clears itself on unmount**, unlike `usePageTitle` — most pages have no footer actions, so
  falling back to none on navigation is the safer default; title deliberately requires every page
  to set it explicitly instead.

### 5. Games List screen (`pages/GamesListPage.tsx`)

Replaces the Stage 2 `HomePlaceholder`. Structured as a thin switch between two mutually-exclusive
leaf components — `GamesListContent` (the list itself) and `GamesSearchForm` (the search form,
`components/GamesSearchForm.tsx`) — each owning its own `usePageTitle`/`usePageFooterActions`
calls; the switch itself calls neither (see Decisions log's title-stomping entry for why).

- `GameListItem`/`GameStatusBadge` (`components/`) render each row: formatted date/time
  (`lib/format.ts`'s `formatGameDateTime`), location, status badge, and `Organised by @{tag}` when
  `organiser` is present.
- Search form: status/team-size/date-range filters, plus the "Games I'm In"/"Games I've
  Organised" toggle (visual only — see known gaps). Iterated several times against live feedback
  — see Decisions log for the final always-visible-plain-date-inputs shape and why the persisted
  filter state and the actual request's date bounds are computed separately.
- "New Game" navigates to `/games/new` (see below); "Search" swaps in `GamesSearchForm`.

### 6. Game View screen (`pages/GameViewPage.tsx`)

Route `/games/:id`. Organiser-only inline editing of Location/Start Time/Duration when
`Scheduled` (`TeamSize` is always read-only — it isn't part of `UpdateGameRequestModel`, can't be
changed after creation). Actions: Invite Players/Manage Teams (organiser, `Scheduled` only) and
Record Result (organiser, `Scheduled` only) open `components/RecordResultModal.tsx`; Delete Game
(organiser, any status) opens the shared `Modal` shell. Non-organisers and finished games get a
read-only view with "View Teams" instead of "Manage Teams". Invite Players/Manage Teams/View Teams
are rendered per the diagram but disabled — no Stage 4/5 screen exists yet to route them to.

### 7. New Game screen (`pages/NewGamePage.tsx`)

**Not in any stage's original scope or diagrams** (`docs/ui-design` has nothing for game
creation) — added mid-stage because there was no way to create a game to actually test the rest
of the flow end to end otherwise. Minimal form covering exactly `CreateGameRequestModel`'s fields;
the current authenticated user is always the organiser (no organiser picker exists). Start Time
defaults to the top of the next hour (`lib/format.ts`'s `nextHourStart`) rather than blank.

### 8. My Account screen (`pages/MyAccountPage.tsx`)

Replaces the Stage 2 placeholder. Display Name/Email editing (Mobile Number stays out per the
known future-state gap), Change Tag routing into the existing Stage 2 `ChangeTag` component, and a
Delete Account confirmation (`Modal` shell, "This cannot be undone.") that logs the user out via
the same Auth0 redirect as a voluntary Log Out on success. Header title shows `@{tag}` (matching
`03-my-account.png`, not a generic "My Account" label — see Decisions log), falling back to "My
Account" while still loading. Button order is Change Tag, Log Out, a deliberate gap, then Delete
Account — keeping the destructive action from sitting flush against the benign ones.

### 9. `TextInput` native picker fix (`components/TextInput.tsx`)

`date`/`time`/`datetime-local`/`month`/`week` input types now always render floated instead of
animating based on focus/value — see Decisions log.

### 10. App shell layout (`index.css`, `App.tsx`, `Header.tsx`, `Footer.tsx`)

Header and Footer are now pinned to the viewport edges (fixed height, `shrink-0`); `main` is the
sole scrolling region (`overflow-y-auto`, `min-h-0`) so the scrollbar renders at the actual edge
of the browser window. Previously the whole page — including header/footer — scrolled together.

### 11. Header polish (`components/Header.tsx`, `public/account-settings.png`)

The My Account icon (right side) had never had a real icon — just a decorative placeholder circle
(`HeaderIconButton`'s fallback when no `children` were passed). Replaced with a real
`account-settings.png` asset the user supplied, styled the same as the home icon (no circular
background pill behind it).

### 12. Pointer cursor on every button (`components/ui/button.tsx` + raw `<button>` sites)

Tailwind's Preflight reset sets `cursor: default` on `<button>` elements — see Decisions log.
`cursor-pointer` added to the shared `Button` component's base classes (covers every screen built
on it) plus the three raw `<button>` elements that don't use it: `Header`'s icon buttons,
`GamesSearchForm`'s `ToggleOption`, and `RecordResultModal`'s option buttons.

### 13. Testing

Vitest + RTL as established. New component/hook/page tests throughout; notable patterns worth
carrying forward:

- `vi.useFakeTimers()` combined with `@testing-library/user-event`'s click/type simulation
  reliably **deadlocks** — see Decisions log for the fix (use `fireEvent` for interaction tests
  that need a fixed system time, or assert date *relationships* instead of literal dates so no
  fake time is needed at all).
- base-ui's `Dialog` marks background content `aria-hidden` while open — a page's own modal
  trigger button drops out of the accessibility tree once the modal opens, so
  `getAllByRole(...)[1]`-style indexing to find "the modal's button" is wrong; there's only ever
  one queryable "Delete Game"/"Delete Account" button at a time.

## Explicitly out of scope for this stage

- Invite Players / Manage Teams screens and the `GET /games/{id}/teams` roster data — Stage 4/5.
- A real Create Game screen design — `NewGamePage` is a deliberately minimal, undesigned
  stand-in built to unblock testing, not a finished Stage 3 deliverable.
- Location-required validation — deferred, see Decisions log.
- Mobile Number on My Account — future state, per `claude.md`'s existing known gap.

## Verification

- `npm run test -- --run` — 171 tests passing.
- `npm run build` and `npm run lint` — clean, no new warnings.
- Manually verified in browser (floating-label fix, fixed header/footer scroll behavior) via the
  public `/dev/components` route — the authenticated flow (create game → view/edit → record
  result/delete → my account) needs a human with real Auth0 credentials to exercise end to end;
  not something this session could drive itself.
- Branch pushed; draft PR not created programmatically (`gh` CLI unavailable in this environment)
  — link handed to the user instead.

## Decisions log

Resolved during implementation (including a pre-branch API-vs-diagram prep pass), kept here for
traceability — outcomes are already reflected inline above.

- **Pre-branch API-vs-diagram conflicts.** `claude.md`'s Modal pattern section describes Record
  Result as a "Home/Away/Draw selector," but `POST /games/{id}/result` only accepts
  `{ winner: "Home" | "Away" | "None" }` — resolved as `Draw` → `winner: "None"` (the API's own
  Swagger examples already label it "Draw"). The diagram's "Complete" status label maps to the
  real `Finished` enum value (naming only, no third status). The games list response envelope is
  `{ data, cursor, count }`, not `{ items, cursor }`. `GetGamesQuery` still doesn't expose
  `OrganiserId`/`UserId` (confirmed still true against current `src/api` source) — the organiser
  toggle stays non-functional as `claude.md` already expected. View Game needs only
  `GET /games/{id}` — team rosters are Stage 4's `GET /games/{id}/teams` concern entirely, never
  fetched here.
- **Game organiser info.** Landed on `main` before this stage branched (`f8cb86a`/`8e22d93`) — a
  nested `GameOrganiserModel { id, tag, displayName }` (nullable) on both `GameModel` and
  `GameDetailModel`. Confirmed in `src/api` source and used directly in `api/games.ts`'s types.
- **`usePageActions` infinite render loop.** The first cut bundled `{ footerActions,
  setFooterActions }` into one context value. Any page calling `usePageFooterActions` with fresh
  JSX every render (near-universal, since the content is usually inline) triggered its own effect
  on every write, because the single context's changing value re-subscribed *writers* too, not
  just the `Footer` reading it. Fixed by splitting into a state context (read by `Footer`) and a
  dispatch context whose value — the `useState` setter — never changes reference, so writers never
  re-render from their own writes. General lesson for this codebase: don't bundle frequently-
  changing state with its setter in one context if both readers and writers exist — split them.
- **Page title/footer "stomping" bug.** `TeamPickerPage` unconditionally called `usePageTitle`
  while conditionally rendering `GamesListPage` (a child) that also called it. React runs a
  child's effects before its parent's on the same commit, so the parent's later effect silently
  overwrote the child's title/footer content — the header would've shown "Pick Your Teams"
  instead of "Games" whenever authenticated. Fixed by restructuring both `TeamPickerPage` and
  `GamesListPage` into thin routing components that themselves never call
  `usePageTitle`/`usePageFooterActions` — only their mutually-exclusive leaf branches
  (`TeamPickerLanding`/`GamesListContent`/`GamesSearchForm`) do. General pattern for later stages:
  a component that conditionally delegates its entire render to one of several child "pages"
  should not also set shared page-chrome state itself.
- **Search panel: overlay → in-place swap.** First built as a `fixed inset-0` full-screen overlay
  with its own header bar and Cancel/Apply footer, stacked on top of the real app chrome — this
  visually fought the actual Header/Footer (doubled-up bars, confusing layering) once live-tested.
  Rebuilt as a plain conditional swap within `GamesListPage` (`GamesSearchForm` replaces
  `GamesListContent`), using the same `usePageTitle`/`usePageFooterActions` mechanism as any other
  screen — no overlay, no duplicate chrome.
- **Search form fields, several rounds of live-testing feedback:**
  - Start From/To were originally checkbox-gated optional `datetime-local` inputs; simplified to
    always-visible plain `date` inputs — nobody filters games by time of day, and a native date
    input already shows its own placeholder mask when empty, so the checkbox added a step without
    adding clarity. Defaults: today through 14 days out.
  - The organiser toggle and date range now persist in the lifted `GamesSearchFilters` state
    (`GamesListPage`), not reset every time the form reopens, even though the organiser toggle is
    still non-functional (`GetGamesQuery` gap) — reopening the form should show what was last
    searched, not a blank slate.
  - The API's date filter is inclusive-from/exclusive-to. The picked "Start To" date needs to be
    fully included, so it's rolled forward by one day (`lib/format.ts`'s `nextDayBoundary`) —
    **only at the point filters become `useGames` query params** (`GamesListContent`), never baked
    into the persisted `GamesSearchFilters` state itself, or reopening the form would show day+1
    instead of what was actually picked.
- **`TextInput` + native date/time pickers.** Safari's native `datetime-local` rendering doesn't
  reliably respect the padding-top the floating-label animation depends on (confirmed live in the
  user's own Safari session — this session's browser tooling is Chromium-only and couldn't
  reproduce it directly, so the fix is based on live user testing, not a direct repro). Fixed by
  forcing `date`/`time`/`datetime-local`/`month`/`week` types to always render floated — a native
  picker already shows its own placeholder mask when empty, so an animated label added no clarity
  anyway, and this keeps every `TextInput` sharing one visual style rather than introducing a
  second static-label layout for these types.
- **Delete-confirmation modal tests.** base-ui's `Dialog` marks background content `aria-hidden`
  while open, so a page's own trigger button (e.g. "Delete Game") drops out of the accessibility
  tree once its confirmation modal is open — `getAllByRole(...)[1]`-style indexing to reach "the
  modal's button" is wrong and flaky; there's only ever one queryable match at a time, so use a
  plain `getByRole` after the modal opens.
- **Fake timers + `userEvent` deadlock.** `vi.useFakeTimers()` combined with
  `@testing-library/user-event`'s click/type simulation reliably times out — `userEvent`'s
  internals depend on real-time scheduling that never resolves once faked without manual
  advancement. Fixed two ways depending on the test: use `fireEvent` (synchronous, no scheduling)
  for interaction tests that need a fixed system time (`GamesSearchForm.test.tsx`), or assert date
  *relationships* instead of literal dates so no fake time is needed at all when a full `userEvent`
  interaction is also required (`GamesListPage.test.tsx`). Also added a defensive
  `afterEach(() => vi.useRealTimers())` in `GamesListPage.test.tsx`, since a test that times out
  before reaching its own cleanup line would otherwise leak fake timers into the next test.
- **My Account header title.** An earlier pass used a generic "My Account" label; corrected after
  the user pointed out `03-my-account.png` actually shows `@MyTag` as the header. Falls back to
  "My Account" while `useSelf` is still pending.
- **My Account button order.** Not diagram-specified; direct user request — Change Tag, Log Out,
  a deliberate gap (~one button's height), then the destructive Delete Account button, so it isn't
  sitting flush against the benign actions.
- **Delete Account reuses the voluntary-logout redirect.** Rather than a bespoke "account deleted"
  page, a successful delete calls the exact same `useAuth0().logout()` redirect (same `returnTo`/
  query-param/toast mechanism already built in Stage 2) — the user is logged out either way, and
  the existing "You've been logged out" toast on return already covers it.
- **Location-required validation — deferred, by user, to later.** Neither the frontend nor
  `CreateGameCommandValidator`/`UpdateGameCommandValidator` currently require `Location` (only
  `MaximumLength(100)`). User's own words: "I feel like it's too easy to spam games, and that
  might slow people down a bit" — deliberate friction against spamming game creation, not a
  data-integrity concern. Decided to add a `NotEmpty` rule on the backend later, not as part of
  this stage, and explicitly not to add a matching frontend guard proactively. The frontend
  already renders inline `Location` errors from any 422 on both `NewGamePage` and `GameViewPage`,
  so no frontend change is needed once the backend rule lands.
- **My Account icon.** `HeaderIconButton`'s right-side slot had never had a real icon, just a
  decorative placeholder circle rendered when no `children` were passed. User supplied a real
  `account-settings.png` (placed in `public/`); wired in with the same plain styling as the home
  icon (no circular background pill) once asked to remove that background.
- **`cursor-pointer` on every button.** Tailwind's Preflight base reset sets `cursor: default` on
  `<button>` elements (a deliberate normalize.css-style choice, not a bug), so none of this app's
  buttons showed a pointer cursor without an explicit override. Added to the shared `Button`
  component's base classes (`components/ui/button.tsx` — one of the few edits made to an
  otherwise-vendor shadcn file, justified since it's the single source of truth for virtually
  every button in the app) plus the three raw `<button>` sites that don't use it. General note for
  later stages: any *new* raw `<button>` element (not going through the shared `Button` component)
  needs its own explicit `cursor-pointer` — it isn't inherited from anywhere global.
