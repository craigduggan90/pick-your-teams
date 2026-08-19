# Pick Your Teams — UI Project Context

The API is fully built and tested. This file is project context for building the frontend from
scratch — read it in full before starting any stage.

**Once a stage has been built, `docs/claude/stage-N.md` records what actually happened** —
context, approach, and a "Decisions log" of anything resolved or discovered mid-implementation
that isn't reflected here. Read the docs for every completed stage before starting the next one;
they carry real implementation detail (exact component/route names, bugs hit and fixed, patterns
later stages should follow) that this file doesn't and won't be updated to include.

## Stack

- React + Vite + TypeScript
- Tailwind + shadcn/ui (unstyled Radix primitives — destructive/custom-styled options like
  "Remove" in a select aren't fighting native browser chrome)
- TanStack Query for all data fetching/caching/mutations
- react-router, nested routes matching the screen hierarchy (Game → Players → Invite is 3 real
  route levels, not tab state — back/forward and browser history need to actually work)
- Vitest + React Testing Library for unit/component tests
- Playwright for E2E

Keep non-visual code (API client, TanStack Query hooks, validation) in framework-agnostic modules
(`api/`, `hooks/`, `lib/`) separate from `components/`/`pages/`. Costs nothing now, avoids a
rewrite if this ever needs to become a native app later.

## Design reference

Excalidraw screen designs live in `docs/ui-design/` (exported PNGs + the source `.excalidraw`
file). Treat them as the visual reference **except** where this file explicitly overrides them —
see "Known diagram/brief conflicts" below. Flag anything that doesn't match the diagrams as you
go rather than silently reconciling it.

## Auth model — not what the diagrams imply

There is no custom backend-for-frontend. In production, the SPA talks directly to an AWS API
Gateway with a custom authorizer attached. The authorizer validates the Auth0-issued token,
resolves it to a `User` (auto-creating one on first login if none exists — new users get
`Tag == Id` by construction, which is the signal for "needs to set a tag"), and injects
`Teams-User-Id`/`Teams-User-Tag`/`Teams-User-Name` headers before the request reaches the API.
The SPA never sees, sets, or needs to know about those headers.

Practical implications:

- Use standard `auth0-react` — PKCE, in-memory access token, `Authorization: Bearer` on every
  API call. No session cookie, no custom auth server.
- "Not logged in" is handled entirely client-side — a route guard (`withAuthenticationRequired`
  or equivalent) blocks navigation before any API call happens. There's no server-side redirect
  to design around; API Gateway can't issue one.
- On load, call `GET /users/self` (returns `UserDetailModel`). If `Id == Tag`, redirect into the
  tag-setup flow.
- **Tag-setup is a hard gate, not a skippable step.** Until a tag is set, the user is redirected
  back into tag-setup at every login. The diagram's "Not Now" button on that screen is not part
  of the intended flow — omit it, or if kept for some other reason it must not actually let the
  user past the gate.
- E2E tests can bypass all of this and hit the API directly with the `Teams-User-*` headers set
  manually — that's how the integration test suite already works, and it's a legitimate trust
  boundary to test against directly.
- **The tag-setup screen is dual-use, not gate-only.** `03-my-account.png`'s "Change Tag" button
  ("Send to Change Tag") routes into the same screen as the Stage 2 gate flow, and it's backed by
  the same `PATCH /users/{id}` endpoint (`Tag` is just one of the fields it accepts alongside
  `DisplayName`/`Email`/`Mobile`). Build the tag-setup component in Stage 2 generically enough to
  be entered either as a hard gate (no back/cancel, forces a tag) or as a normal editable screen
  from My Account (Back enabled, no redirect-on-skip) — don't hard-code the gate behavior into the
  component itself. My Account's "Change Tag" entry point is then just a routing addition in
  Stage 3, not a rebuild.

## Design tokens

- Semantic palette: Primary, Secondary, Tertiary, Success, Warning, Error, Info, plus a Dark Grey
  (body text) and Light Grey (disabled/placeholder). Header bar uses Primary.
- No custom typography requirement — standard sans-serif, system font stack or a well-supported
  open font.
- **Floating labels are a core TextInput behavior**, not diagram flourish — `02-games-list.png`
  ("Do the floaty labels thing when someone puts a value in") and `03-my-account.png`
  ("Repeating for posterity - floaty labels") both call it out. Build it into the Stage 1
  TextInput primitive so every screen gets it for free.
- Mobile-first, not mobile-only. Every screen in the diagrams is phone-width; that's the primary
  target, but the layout should hold up reasonably at desktop widths too (a centered fixed-width
  column is fine for v1 — no bespoke desktop layouts yet).

## Known gaps — don't block on these, they're deliberate

- `GetGamesQuery` doesn't expose `OrganiserId`/`UserId` yet even though the repository already
  supports both as flat params — "Games I'm In" / "Games I've Organised" toggle needs that wired
  through before it can work. Build the toggle UI, but it's non-functional until the backend gap
  closes.
- Pagination is forward-cursor-only, no true "previous page." The Games list uses a "load more"
  button when `Cursor != null`, not prev/next controls.
- `DeletePlayer` already supports self-removal (organiser-or-self), but no UI control exists for
  a player to remove themselves from a game they're in. Worth adding once the Game view screen
  is built.
- No screen exists yet for viewing a single invitation via a direct email link (sign in → land on
  `GET /invitations/{id}` with full game + organiser info). `GetInvitationById` already returns
  everything needed for it; it's just not designed yet. Later addition, not part of initial build.
- **Mobile number is future state.** `03-my-account.png` shows a Mobile Number field with a
  country-code picker; we don't need it yet and the API may not support it. Leave it out of the
  Stage 3 build.
- The "working" transitional row state on `07-my-invitations.png` (an invitation row dims right
  after tapping accept/decline, before it disappears) is diagram illustration, not a v1
  requirement. A simple loading/disabled state on the row's buttons during the request is enough
  for the Stage 5 build.

## Modal pattern — reuse this, don't redesign per-screen

Only one modal is actually rendered in the diagrams: **"Remove @Tag?"** on `06-a-view-teams.png`
— title, body copy, Cancel + destructive-red primary button, centered card. Two other places
reference a modal without drawing one, and should reuse this same shell rather than get a bespoke
design:

- **Delete Account** (`03-my-account.png`) — only annotated as `"Popup/modal: 'This cannot be
  undone'"`, no rendered box. Build it as: title, the cannot-be-undone copy, Cancel + destructive
  Delete Account button.
- **Record Result** (`04-view-game.png`) — shown only as an organiser-only action button
  alongside Invite Players/Manage Teams/Delete Game; no dedicated screen or modal was designed.
  Build it as the same shell with a Home/Away/Draw selector as the content (matching the
  `Winner: Home Team!` state already shown on the finished-game view) instead of confirmation
  text, plus Confirm/Cancel.

Both should use the shared Modal primitive from Stage 1 — no new visual design needed.

## Invitations — build against this, not the stale diagram

`docs/ui-design/05-invite-players.png` is stale. The original design had a mixed tag-or-email
invite list with per-row claim tracking; that whole feature was deleted and replaced with a
dedicated `Invitations` resource, **tag-only for v1** (no inviting someone without an existing
account):

- `POST /invitations` — body `{ gameId, userTags: string[] }`. Organiser-only. All-or-nothing: if
  any tag doesn't resolve to a real user, the whole request 422s naming the specific bad tag(s),
  nothing gets created. Errors come back as one message per bad tag (`'{tag}' is not a valid
  tag.` / `Tag not found: {tag}` / duplicate-tag / empty-list) — render as a list at the bottom
  of the form; no need to match errors back to specific input rows by index.
- `GET /invitations` — filterable by `gameId`, `userId`, `emailAddress`, `status`, date ranges.
  Ownership-guarded: filtering by `userId` requires the actor to *be* that user; filtering by
  `gameId` requires the actor to be that game's organiser. Filtering by neither is currently
  unrestricted (known gap, not urgent).
- `POST /invitations/{id}` — accept. `DELETE /invitations/{id}` — decline. Both self-only.
  Idempotent if repeating the same outcome (204, no-op). 422 if the invitation already resolved
  to the *opposite* outcome or errored. If the invitee already has a `Player` row in that game by
  the time they act, the invitation quietly moves to a `Failed` status server-side but the
  endpoint still returns success — don't surface an error to the user for this case, they got
  what they wanted either way.
- Resolved invitations (accepted/declined) don't reappear on subsequent `GET /invitations` loads
  for "My Invitations" — no need to filter them out client-side, ask for `status=Open` if that's
  the only thing you want to show.

## Manage Teams — control shape and other clarifications

- The team-assignment control per player is **a single `<select>`-style dropdown** (native or
  Radix), and its options are current-state-aware — a Home player sees `[Away, No team, Remove]`,
  not all three teams plus their own.
  - `06-a-view-teams.png` shows this instead as a `[...]` icon button that opens a custom popup
    menu with conditional items (e.g. Home row → "To Away Team" / "Remove from Team" / "Remove
    from Game"). **That widget is superseded — build a `<select>`, not a context menu.** The
    underlying logic in the diagram (which options are available per current team, "Remove from
    Game" as the destructive option) is correct and can be built from directly; only the trigger
    control changes.
- Removing a User-linked player (not a Dummy) triggers a confirmation modal — Dummy players can
  just be re-added, no friction needed.
- The modal copy needs `@{tag}` pulled from `player.User?.Tag` specifically, not whatever
  `DisplayName` resolves to (those are genuinely different fields once a player has no name
  snapshot and falls back to their live user profile).
- "Reset" is not "Clear" — that would be "Unassign All," which we don't have. "Reset" returns the
  player list to the current `GetTeams` value. Moving players between teams doesn't count until
  someone hits Save.

## Screen-specific notes from the diagrams (useful, not in the original brief text)

- `04-view-game.png`: Invite Players, Manage Teams (edit), Record Result, and Delete Game are all
  **organiser-only**. A non-organiser sees a read-only "View Teams" in that slot instead of
  "Manage Teams." Bake this into route/component permission logic in Stage 3/4 rather than
  discovering it later.

## Suggested build order — five stages, each independently reviewable

**Stage 1 — Foundations.** Tailwind theme wired to the token list above, Vitest + RTL configured,
a handful of primitive components with tests (Button, TextInput w/ error state, Toast, Modal,
Select, and the shared header/footer shell). No real screens yet.

**Stage 2 — Auth + tag setup.** `auth0-react` wired in, route guard for unauthenticated access,
`GET /users/self` integration, and the tag-setup screen with its four states
(initial/loading/error/success) — hard gate, no skip. Build the tag-setup component in gate/normal
dual-use mode (see "Auth model" above) so Stage 3 can reuse it for "Change Tag."

**Stage 3 — Games list + Game view + My Account.** Home screen (list, loading/empty/results
states, search panel — organiser/participant toggle is non-functional pending the backend gap),
View Game screen (with organiser-only action gating and the Record Result modal — see "Modal
pattern" above), My Account screen (no mobile number field) including the delete-account
confirmation modal and a "Change Tag" button routing into the Stage 2 `ChangeTag` component (see
`docs/claude/stage-2.md` — it was renamed from "tag-setup" once it became genuinely dual-use) in
its normal (non-gate) mode.

**Already exists, ahead of schedule:** Stage 2 needed a real page to link to `/change-tag` a
second time after the first gate pass, so a minimal `MyAccountPage` at `/account` already exists
— one line of placeholder copy plus a working "Change Tag" button, wired exactly the way this
stage's real one should be. Stage 3 replaces its contents (profile fields, delete-account modal)
rather than building the route/page/Change-Tag-wiring from scratch — see `docs/claude/stage-2.md`
section 7 for exactly what's there now.

**Stage 4 — Teams management.** View/Manage Teams screen, the per-row assignment `<select>`,
Generate/Reset/Save, Add Non-User Player.

**Stage 5 — Invitations.** Create Invitations form (against the tag-only contract above, not the
stale diagram), My Invitations screen with accept/decline.

## Workflow

- One branch per stage (e.g. `stage-1-foundations`), branched off the latest `main`.
- Commit in small, logical chunks within a stage rather than one commit at the end.
- Push the branch and open a **draft PR** — never push directly to `main`. Existing `.husky`
  hooks and any CI in `.github` should run against the PR.
- Wait for review/merge before branching the next stage off the updated `main`.

Decisions log
Clarifying questions raised during design review, kept here for traceability. The resolved outcomes are already reflected inline in the sections above — this is the "why," not new instructions.
- Tag-setup "Not Now" button — the diagram shows a skip option on the tag-setup screen; is it a soft nag or a hard gate? → Hard gate. Until a tag is set, the user is redirected back into tag-setup at every login. The button doesn't belong in the intended flow.
- Mobile Number field — 03-my-account.png shows a Mobile Number field with a country-code picker that isn't mentioned anywhere in the original brief; in scope? → No, future state. Not needed yet, may not even be supported by the API yet. Omit from the Stage 3 build.
- Record Result screen — no dedicated diagram exists for entering a result/winner, even though it's shown as an organiser-only action on 04-view-game.png; in or out of scope? → In scope, no new design needed. Reuse the Modal primitive and the visual shell from the "Remove @Tag?" modal (06-a-view-teams.png) with a Home/Away/Draw selector as the content instead of confirmation text.
- Manage Teams control shape — 06-a-view-teams.png shows a [...] icon button opening a custom popup menu per player row; does that match the brief's stated <select>-style dropdown? → No, the diagram's widget is superseded. Build a single <select> with current-state-aware options; the underlying logic (which options appear per current team) carries over unchanged.
- Delete Account modal — 03-my-account.png only annotates a popup/modal, doesn't render one; does it need its own design? → No, same as Record Result — reuse the "Remove @Tag?" shell with the cannot-be-undone copy as content.
- Floating labels — 02-games-list.png and 03-my-account.png both call out a "floaty labels" interaction that isn't mentioned in the design-tokens section; is it required? → Yes. Build it into the Stage 1 TextInput primitive as a core behavior, not a later enhancement.
- Change Tag button — 03-my-account.png has a "Send to Change Tag" button not mentioned anywhere in the build order or known gaps, backed by the same PATCH /users/{id} endpoint as tag-setup; in scope, and if so when? → In scope, and it belongs to the login/tag path from Stage 2 onward, not bolted on in Stage 3. Build the Stage 2 tag-setup component generically (gate mode vs. normal editable mode) so Stage 3's My Account can route into it without a rebuild.
- My Invitations transitional row state — 07-my-invitations.png shows a dimmed "working" state on a row between tapping accept/decline and the row disappearing; is this optimistic UI required for the Stage 5 build? → No, nice-to-have only. A simple loading/disabled state on the row's buttons is enough for v1.