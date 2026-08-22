# Pick Your Teams — Project Context

The original 5-stage frontend build (plus a Stage 6 addendum) is complete and merged to `main`.
This file now records the architecture, established patterns, and conventions the codebase
actually follows — read it before making changes. The old `docs/claude/stage-N.md` docs that
narrated that build stage-by-stage have been consolidated into this file and removed; don't look
for them.

## Stack

- React + Vite + TypeScript
- Tailwind + shadcn/ui (unstyled Radix primitives — destructive/custom-styled options like
  "Remove" in a select aren't fighting native browser chrome)
- TanStack Query for all data fetching/caching/mutations
- react-router, nested routes matching the screen hierarchy (Game → Players → Invite is 3 real
  route levels, not tab state — back/forward and browser history need to actually work)
- Vitest + React Testing Library for unit/component tests
- Playwright for E2E — not built yet; see the "End-to-end testing" section below for the current
  plan and what already exists to support it.

Keep non-visual code (API client, TanStack Query hooks, validation) in framework-agnostic modules
(`api/`, `hooks/`, `lib/`) separate from `components/`/`pages/`. Costs nothing now, avoids a
rewrite if this ever needs to become a native app later.

## Design reference

Excalidraw screen designs live in `docs/ui-design/` (exported PNGs + the source `.excalidraw`
file). Treat them as the visual reference **except** where this file explicitly overrides them —
see "Screen reference" below. Flag anything that doesn't match the diagrams as you go rather than
silently reconciling it.

## Current routes

- `/` — Team Picker: public landing (Log In / Register) when unauthenticated; redirects onward
  through the tag-check when already authenticated.
- `/change-tag` — dual-use: hard gate (no Back, forced) when `Id === Tag`, or a normal "Change
  Tag" entry from My Account otherwise. Wrapped in `RequireAuth` only, not `RequireAuthAndTag` —
  gating it on having a tag would create a redirect loop on the very screen that sets one.
- `/account` — My Account: profile fields, Change Tag, Log Out, Delete Account.
- `/games/new` — minimal, undesigned Create Game form (no diagram exists for this; added ahead of
  schedule just to unblock testing the rest of the flow).
- `/games/:id` — Game View / "Manage Game": organiser-only edit of Location/Start Time/Duration,
  Record Result, Delete Game, View Invites. Reached only via the Teams screen's "Manage Game"
  link (organiser-only) — never linked directly from the games list.
- `/games/:id/teams` — **the default landing when a game is tapped from the list** (people spend
  more time here than on Game View). View/Manage Teams, the per-player `<select>`,
  Generate/Reset/Save, Add Non-User Player, Invite Players (organiser + `Scheduled` only).
- `/games/:id/invite` — Create Invitations (tag-only), organiser + `Scheduled` only, reached only
  from the Teams screen.
- `/games/:id/invites` — View Invites (list + status per invitation), organiser-only, any game
  status, reached from Game View.
- `/invitations` — My Invitations (accept/decline your own open invitations), reached from the
  persistent Header icon; the icon's badge lights up from `GET /users/self`'s live
  `pendingInvitations` count.
- `/dev/components` — public, permanent component showcase page. Kept intentionally, not a
  throwaway — doubles as an ongoing visual reference as primitives are added.

## Established patterns — follow these for new screens

- **Every routed page calls `usePageTitle(title)`** (`hooks/usePageTitle.tsx`). There's no
  automatic reset when navigating away from a page that set a custom title, so skipping this
  leaves the previous page's title showing.
- **Footer action buttons go through `usePageFooterActions(node)`** (`hooks/usePageActions.tsx`).
  Unlike the title hook, this clears itself on unmount — most pages have no footer actions, so
  that's the safer default.
- A component that conditionally renders one of several child "pages" (a router-like switch) must
  not itself call `usePageTitle`/`usePageFooterActions` — only the leaf branches should. React
  runs a child's effects before its parent's on the same commit, so the parent's effect would run
  later and silently overwrite whichever child is actually showing.
- **API response bodies are camelCase; request bodies and query params stay PascalCase**,
  matching the C# DTOs directly (ASP.NET's model binding is case-insensitive on input, so this
  works). Don't assume either casing for a new endpoint — check the real wire shape against source
  or curl first; this asymmetry has caused real bugs.
- **`apiFetch` (`api/client.ts`) treats any empty response body as `undefined`, not just a 204.**
  Some endpoints (`CreateInvitations`) return a bare `201` with no body — assuming only 204 is
  empty caused a real bug where a successful request was reported to the user as a generic error.
- **The confirmation-dialog / bottom-sheet primitive is `Sheet` (`components/Sheet.tsx`), not
  `Modal`.** `Modal` (a centered dialog) was removed and replaced everywhere with `Sheet` (slides
  up from the bottom); same prop shape, drop-in replacement.
- **Team color coding**: Home = Primary, Away = Secondary. A row with an unsaved pending move
  renders at reduced color strength plus a small red corner flag. Whether a row counts as
  "pending" is a comparison of each player's *effective* team against their last-*saved* bucket —
  not overlay-presence, which breaks after Generate (it rebuilds the overlay for every player,
  seeded-and-unchanged included).
- **`TextInput` always renders its label floated** for `date`/`time`/`datetime-local`/`month`/
  `week` input types — native pickers show their own placeholder mask regardless of value, and
  Safari doesn't reliably respect the padding-top the floating animation depends on.
- Every raw `<button>` that doesn't go through the shared `Button` component needs its own
  explicit `cursor-pointer` class. Tailwind's Preflight reset sets `cursor: default` on all
  buttons; nothing restores it globally except `Button`'s own base classes.

## Auth model — not what the diagrams imply

There is no custom backend-for-frontend. In production, the SPA talks directly to an AWS API
Gateway with a custom authorizer attached. The authorizer validates the Auth0-issued token,
resolves it to a `User` (auto-creating one on first login if none exists — new users get
`Tag == Id` by construction, which is the signal for "needs to set a tag"), and injects
`Teams-User-Id`/`Teams-User-Tag`/`Teams-User-Name` headers before the request reaches the API.
**The SPA never sees, sets, or needs to know about those headers** — it only ever sends the real
Auth0 bearer token (`apiFetch` always sets `Authorization: Bearer <token>`).

Practical implications:

- Use standard `auth0-react` — PKCE, in-memory access token (`cacheLocation: "memory"`, chosen
  deliberately for XSS resistance over the convenience of `localstorage`), `Authorization: Bearer`
  on every API call. No session cookie, no custom auth server.
- "Not logged in" is handled entirely client-side — a route guard (`RequireAuth`/
  `RequireAuthAndTag`) blocks navigation before any API call happens. There's no server-side
  redirect to design around; API Gateway can't issue one.
- On load, call `GET /users/self` (returns `UserDetailModel`). If `Id == Tag`, redirect into the
  tag-setup flow.
- **Tag-setup is a hard gate, not a skippable step.** Until a tag is set, the user is redirected
  back into tag-setup at every login.

**No Lambda authorizer exists as deployable code yet** — that's still "on the list." Locally, this
means:

- Plain `Teams.Api` (`dotnet run` from `src/api/Teams.Api`) currently has nothing to turn a bearer
  token into `Teams-User-*` headers, so it doesn't work for interactive local dev right now. This
  is a known, accepted, temporary state — don't try to fix it by teaching the UI about headers
  again (that workaround existed once, via a header-injecting Vite dev proxy, and was deliberately
  removed once the alternative below existed).
- **Run `Teams.Api.EndToEndTests` instead** (`src/api/Teams.Api.EndToEndTests`, applicationUrl
  `:5230`) for anything needing a working local backend. It runs the real API (via the same
  `Startup.ConfigureTeamsServices`/`ConfigureTeamsApplication` extension methods `Teams.Api`'s own
  `Program.cs` uses) against a real SQLite file that's wiped and re-migrated on every startup,
  seeded with 25 fixed users (`user-001`…`user-025`). `Teams.Api` never references this project,
  so none of its test-only code can end up in a production build.
- **`ActorResolverMiddleware`** (in that project) stands in for the Lambda authorizer: it reads
  `Authorization: Bearer <id>`, resolves `id` against the seeded database, and sets the
  `Teams-User-*` headers `ActorAccessor` already expects — leaving them unset if the id doesn't
  resolve to a real user. It deliberately only understands a raw seeded-user id as the "token,"
  not a real Auth0 JWT — testing with your own real Auth0 account against this stand-in isn't
  supported, and isn't planned to be until the real authorizer exists.
- The Vite dev proxy (`vite.config.ts`) still routes `/api` to avoid a CORS-blocked cross-origin
  call (the API has no CORS config), but no longer touches headers — whatever `Authorization` the
  browser sends passes through untouched.

## End-to-end testing

Planned, not yet built: a Playwright project at `src/e2e` (sibling to `src/api`/`src/ui`), driving
a locally-running UI dev server plus `Teams.Api.EndToEndTests`. Agreed direction so far:

- Auth is faked via a Vite `resolve.alias` swap of the bare `@auth0/auth0-react` import specifier
  to a module owned entirely by `src/e2e` — zero changes to `src/ui/src/**`. The app's whole usage
  surface is narrow: `Auth0Provider`, `useAuth0` (only `isAuthenticated`, `isLoading`,
  `getAccessTokenSilently`, `loginWithRedirect`, `logout` are ever destructured), and the
  `AppState` type.
- The fake needs to be *controllable* from test code mid-test (e.g. state exposed on `window`,
  driven via `page.evaluate`) so a single test can log in as user A, act, log out, log in as user
  B — not just one fixed identity for the whole run.
- `getAccessTokenSilently()` in the fake resolves to the seeded user's raw id string, which
  `ActorResolverMiddleware` (see "Auth model" above) already knows how to turn into headers.

## Design tokens

- Semantic palette: Primary, Secondary, Tertiary, Success, Warning, Error, Info, plus a Dark Grey
  (body text) and Light Grey (disabled/placeholder). Header bar uses Primary. Current hex values
  are explicitly temporary placeholders, not final brand colors — re-check before treating any
  contrast reasoning as settled.
- No custom typography requirement — standard sans-serif, system font stack.
- **Floating labels are a core `TextInput` behavior**, built into the primitive so every screen
  gets it for free.
- Mobile-first, not mobile-only. A centered fixed-max-width column handles desktop widths — no
  bespoke desktop layouts.

## Known gaps — deliberate, don't block on these

- `GetGamesQuery` doesn't expose `OrganiserId`/`UserId` yet even though the repository already
  supports both as flat params — the Games list's "Games I'm In" / "Games I've Organised" toggle
  is built but non-functional until the backend gap closes.
- Pagination is forward-cursor-only, no true "previous page" — lists use a "Load More" button.
- `DeletePlayer` already supports self-removal (organiser-or-self), but no UI control exists for a
  player to remove themselves from a game they're in.
- No screen exists for viewing a single invitation via a direct email link (`GET
  /invitations/{id}` already returns everything needed for it).
- **Mobile number is future state** — `03-my-account.png` shows a field for it; the API may not
  even support it yet. Left out of My Account.
- No client-side guard against inviting a tag who's already a player, already has an Open
  invitation to the same game, or inviting yourself — the backend doesn't validate these either.

## Screen reference — diagram conflicts, resolved

- **Modal pattern.** Only one confirmation dialog was actually designed in the diagrams
  (`06-a-view-teams.png`'s "Remove @Tag?" — title, body, Cancel + destructive button). Delete
  Account and Record Result each reuse that same shell (`Sheet`, see "Established patterns" above)
  rather than getting bespoke designs — Record Result's content is a Home/Away/Draw selector
  instead of confirmation text.
- **Invitations — build against this, not `05-invite-players.png`, which is stale.** The original
  design had a mixed tag-or-email invite list with per-row claim tracking; that whole feature was
  deleted from the API and replaced with a dedicated `Invitations` resource, tag-only, no inviting
  someone without an existing account. `POST /invitations` is all-or-nothing (any bad tag 422s the
  whole request); errors render as a flat list at the bottom of the form, not mapped back to
  specific rows. `POST`/`DELETE /invitations/{id}` (accept/decline) are self-only and idempotent
  on repeating the same outcome. If the invitee already has a `Player` row in that game by the
  time they act, the invitation quietly moves to `Failed` server-side but the endpoint still
  returns success — don't surface an error for that case.
- **Manage Teams control shape.** `06-a-view-teams.png` shows a `[...]` icon opening a custom
  popup menu; that widget is superseded — it's a single `<select>` with current-state-aware
  options (a Home player is offered Away/Unassign/Remove, never "Home" itself). The underlying
  logic (which options are available per current team) carries over from the diagram unchanged.
  Removing a User-linked player (has a `Tag`) triggers a confirmation `Sheet`; a Dummy player is
  deleted with no confirmation. "Reset" reverts to the last-fetched `GetTeams` value, not an
  "Unassign All" — moving players doesn't count until Save.
- **Organiser-only actions.** Invite Players, Manage Teams (edit), Record Result, View Invites,
  and Delete Game are all organiser-only. A non-organiser sees read-only "View Teams" instead of
  "Manage Teams," and no Game View link at all (the Teams screen's Game Details sheet already
  shows them everything read-only).

## Workflow

- One branch per unit of work, branched off the latest `main`.
- **A backend-only change gets its own branch, separate from any frontend work that depends on
  it, merged first.** (e.g. `backend/stage4-prep-changes`, `backend/stage6-invitee-field`.) Keeps
  the backend PR reviewable on its own, and means a dependent frontend branch only ever builds
  against a merged, real API contract — never speculative shared history.
- Commit in small, logical chunks rather than one commit at the end.
- Push the branch and open a **draft PR** — never push directly to `main`. Existing `.husky` hooks
  and CI should run against the PR.
- Wait for review/merge before branching further work off the updated `main`.
