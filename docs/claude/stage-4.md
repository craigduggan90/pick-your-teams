# Stage 4 — Teams Management

## Context

Stage 3 (`stage-3-games-my-account`, merged to `main`) built `GameViewPage` with "Manage Teams"/
"View Teams" buttons rendered but `disabled`, explicitly waiting on this stage. Stage 4 is
`claude.md`'s next step: the Teams screen from `06-a-view-teams.png`/`06-b-view-teams.png` — View/
Manage Teams, the per-player `<select>`, Generate/Reset/Save, and Add Non-User Player.

A prep pass ahead of branching (read `claude.md`, `docs/claude/stage-1.md`–`stage-3.md` in full,
and both Teams diagrams, cross-checked against the actual API source) found three real gaps
between the diagrams and what the API actually returned:

1. `PlayerModel`/`PlayerDetailModel`/`GameTeamPlayerModel` exposed `UserId` but never `Tag` — the
   diagrams show every row as `DisplayName [(@Tag)]`, and there's no batch user-lookup endpoint
   to backfill it client-side.
2. `GameTeamsModel` (`GET /games/{id}/teams`) was just `{ Id, Home, Away }` — no "Unassigned"
   bucket, even though both diagrams show one.
3. Nothing capped a game's roster at `TeamSize * 2` — `CreatePlayer`/`CreateDummyPlayer`/
   `AcceptInvitation` would let sign-ups exceed what the two teams could actually hold, which
   `SetTeams` would then reject anyway, just much later and more confusingly.

All three were fixed on the backend first (user's own change, PR #8 "feat: stage 4 backend
changes", `1652bf7`, merged to `main` before this branch started) rather than worked around on the
frontend — `Tag` added (nullable) to all three response models, `GameTeamsModel` gained
`Unassigned: GameTeamPlayerModel[]`, and the three player-creation paths now 422 once
`game.Players.Count >= game.MaxPlayers`. Verified directly against the merged source, not just
taken on trust.

`stage-3-games-my-account` was 4 commits ahead / 9 behind `main` at that point (branched before the
backend PR merged) — rebased onto `main` before branching Stage 4 off it; clean rebase, no
conflicts, since the backend changes were additive-only new fields.

## Approach

### 1. API layer (`src/ui/src/api/`)

`games.ts` gained `GameTeamPlayerModel`/`GameTeamModel`/`GameTeamsModel` types and
`getGameTeams`/`setGameTeams`/`generateGameTeams`, matching the real wire shapes confirmed against
the merged backend source (camelCase responses, PascalCase request bodies — same convention as
every other module). New `players.ts` (mirrors `users.ts`'s shape) for `createDummyPlayer`/
`deletePlayer`.

### 2. Hooks (`src/ui/src/hooks/`)

`useGameTeams` (query, key `['gameTeams', id]`), `useSetGameTeams`, `useGenerateGameTeams`,
`useCreateDummyPlayer`, `useDeletePlayer` — same `useAuth0().getAccessTokenSilently` +
`useMutation<T, ApiError, TVariables>` shape as every Stage 3 hook. `useGenerateGameTeams`
deliberately does **not** invalidate any query — it only returns a suggestion for the caller to
fold into local pending state, it never writes anything itself.

### 3. Components

- `TeamRosterRow` — one player row (`DisplayName [(@Tag)]`, Rating, team label), colored by team
  (see "Team color coding" below), and in edit mode a `SelectField` (Stage 1's `components/
  Select.tsx`, which already supported a `destructive` option flag — exactly what "Remove from
  Game" needed, no changes to that primitive required). Options are current-state-aware per
  `claude.md`'s resolved control shape (a Home player is offered Away/Unassign/Remove, never
  "Home" itself). The select is deliberately uncontrolled — it's used as an action menu, not a
  persistent field; see the Decisions log for why that's safe here.
- `TeamRosterSection` — one team's heading (+ live rating in edit mode), rows list.
- `RemovePlayerModal` — reuses the shared `Modal` (same shell as `RecordResultModal`): `` `Remove
  @${tag}?` ``, the diagram's body copy, Cancel + destructive Remove. Only ever mounted for a
  User-linked player (has a `Tag`) — a Dummy player is deleted with no confirmation at all.
- `AddNonUserPlayerForm` — inline collapsible section per `06-b`, not a modal.
- `GameTeamsPage` (route `/games/:id/teams`) — thin wrapper computing `canEdit = isOrganiser &&
  isScheduled` (identical logic to `GameViewPage`), rendering either `ViewTeamsView` (read-only,
  `Back` only) or `EditTeamsView`.

### 4. The pending-assignment overlay (`EditTeamsView`)

Team (re)assignment stays **local until Save** — `Record<playerId, 'Home' | 'Away' | 'None'>`,
empty on mount, layered over the last-fetched server roster rather than a full copied-and-mutated
roster. This is what lets Remove-from-Game and Add-Non-User-Player — both immediate mutations,
both triggering a refetch — coexist safely with unsaved moves for *other* players: a refetch
changes the base roster (a player disappears, or a new one appears in `unassigned`) without
touching the overlay, so nobody's in-progress move gets silently discarded.

- **Reset** → `setOverlay({})`, reverting to the last-fetched server state.
- **Generate** → seeded from the last-***saved*** Home/Away split (`teams.home.players`/
  `teams.away.players` ids from the query data, not the pending overlay), fixed `Differential:
  200`; on success the whole overlay is rebuilt from the response.
- **Save** → builds `HomeTeamIds`/`AwayTeamIds` from the merged roster, `PUT`s once, clears the
  overlay on success.
- Team ratings shown during editing are recomputed live from the merged roster (sum of player
  ratings per bucket), not the server's last-saved `TeamRating` — so the header numbers reflect
  in-progress moves before Save, not stale values.
- Remove-from-Game / Add-Non-User-Player are independent mutations with their own toast feedback,
  never gated on or gating Save.

### 5. Team color coding

Home sections/rows use the `primary` color token, Away use `secondary`, Unassigned stays neutral —
this also happens to match the diagrams' own blue-ish Home / orange-ish Away styling, not purely a
UI-polish add-on. A row whose player id is present in the edit session's overlay (touched since the
last Save, by a manual move or by Generate) renders its team color at reduced strength until Save
succeeds; read-only View Teams has no pending concept and always renders full-strength.

### 6. Wiring

`GameViewPage`'s "Manage Teams"/"View Teams" button (previously `disabled`, per Stage 3) now
navigates to `/games/:id/teams`. New route added in `App.tsx`, guarded by the existing
`RequireAuthAndTag`.

### 7. Testing

New `GameTeamsPage.test.tsx` (mirrors `GameViewPage.test.tsx`'s hook-mocking approach) covers:
loading/error states, read-only rendering for a non-organiser and for a finished game, a `<select>`
move staying pending (no Save call), Save sending the merged ids, Reset discarding a pending move,
Generate seeding from the last-saved split, Remove from Game's modal-vs-immediate split by
tag/no-tag, Add Non-User Player's submit and inline field-error paths. New `TeamRosterRow.test.tsx`
covers the current-state-aware option list directly. `GameViewPage.test.tsx` gained one test for
the new Manage Teams navigation.

## Explicitly out of scope for this stage

- Invite Players — still `disabled`, Stage 5.
- Configurable Generate "competitiveness" (the `Differential` value) — fixed at `200` for v1, no
  UI control; flagged by the user as a future job, not Stage 4 scope.
- A click-outside-cancels behavior on the Add Non-User Player inline form (06-b's "clicking the
  main page discards it as though Cancel was clicked" annotation) — only an explicit Cancel button
  is wired; not worth the added complexity for v1.

## Verification

- `npm run build`, `npm run lint`, `npm run test -- --run` — all clean, 191 tests passing (up from
  171 at the end of Stage 3).
- Browser-verified what's actually checkable without a real Auth0 session: the public landing page
  loads with no console errors, and navigating directly to `/games/:id/teams` while logged out
  correctly redirects to `/` via the existing route guard rather than crashing. The authenticated
  Edit/View Teams flow itself needs a human with real Auth0 credentials to exercise end to end —
  same limitation Stage 3 hit and documented, not something this session could drive itself.
- Branch pushed as `stage-4-teams-management`; draft PR not created programmatically (`gh` CLI
  unavailable in this environment) — link handed to the user instead.

## Decisions log

Resolved during prep and implementation, kept here for traceability — outcomes are already
reflected inline above.

- **Backend gaps found in prep, fixed on the backend rather than worked around.** See Context —
  `Tag` on player/team models, `Unassigned` on `GetTeams`, and the roster-size cap were all real
  gaps against the diagrams, not things to silently reconcile client-side. All three landed as
  `main`'s PR #8 before this branch started; verified directly against the merged source.
- **Save semantics — team (re)assignment stays local/pending until Save.** The only assignment
  endpoint that exists is the bulk `PUT /games/{id}/teams`; there's no per-player `PATCH` for team.
  The diagram's own annotation is explicit about this: *"Reset... returns the player list to the
  current GetTeams value. Moving players between teams won't count until someone hits save."* A
  per-move-immediate model would need a new backend endpoint and would make Reset/Save themselves
  vestigial.
- **Remove from Game and Add Non-User Player are the exception — both immediate, not pending.**
  Both act on the player/roster itself (`DELETE /players/{id}`, `POST /players/dummy`), not team
  assignment, and the diagrams give each its own dedicated Saving/Error/Success states distinct
  from the main Edit Teams Save flow — a deliberate distinction, not diagram noise.
- **Generate's `Differential` — fixed at `200`, no UI control.** Matches the API's own Swagger
  example. Configurable "competitiveness" explicitly flagged by the user as future scope, not this
  stage.
- **Generate's seeding — only players already *saved* to a team are fixed.** Initially built
  against "full reshuffle every time" (empty seed arrays) as the simplest default; the user
  overrode this with a specific rule mid-build: seeds come from the **last-fetched server**
  `home`/`away` lists (i.e. what was actually committed on the last Save), not the client-side
  pending overlay. A player saved Home stays seeded Home even if the organiser has since (unsaved)
  dragged them toward Away in the pending state; anyone only pending-assigned, or still
  unassigned, is fair game for Generate to reshuffle.
- **Team color coding — Home = Primary, Away = Secondary, plus a pending/saved visual
  distinction.** Raised as an open question by the user (worried that, without one, Generate/
  manual moves would look identical to what's actually committed). Two options were offered — a
  faint tint on changed rows only, or full home/away color-coding with saved-vs-pending strength —
  the user picked the bigger option. Resolved as: Home/Away color-coded throughout (matches the
  diagrams' own blue/orange styling, not just a UI-polish add-on), with pending rows at reduced
  color strength until Save. `isPending` is a simple "is this player id present in the overlay"
  check, not a deep-equality check against the original bucket (a Home→Away→Home ping-pong in one
  session still shows pending) — an accepted v1 simplification.
- **The `<select>` is deliberately uncontrolled.** Considered making it a controlled component
  showing the current team as its value, but base-ui's `Select` (like most headless-UI select
  primitives) only stays continuously controlled if `value` is non-`undefined` on every render —
  and `claude.md`'s current-state-aware requirement means the current team is deliberately *not*
  one of the listed options, so a controlled `value` pointing at an unlisted item risks not
  resolving to a label at all. Left fully uncontrolled instead (used purely as an action menu, no
  `value` prop) — this works cleanly because rows are rendered via three separate section
  `.map()`s (Home/Away/Unassigned), so a team-changing pick moves the row into a different
  section's list on the next render, which is a genuine remount (different parent subtree, not
  just a reordered sibling) and resets the select back to its placeholder for free. The row's own
  plain-text team label (not the select) is what actually displays current state — the select
  never needs to.
- **Live-recomputed team ratings during editing.** The server's `TeamRating` only reflects the
  last save; showing it unchanged while the organiser moves players around client-side would look
  wrong. Recomputed as a simple sum of the merged roster's player ratings per team instead — cheap
  given every `GameTeamPlayerModel` already carries its own `rating`.
