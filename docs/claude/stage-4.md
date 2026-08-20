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

### 5. Team color coding, and the pending/saved distinction

Home sections/rows use the `primary` color token, Away use `secondary`, Unassigned stays neutral —
this also happens to match the diagrams' own blue-ish Home / orange-ish Away styling, not purely a
UI-polish add-on. A row whose player id is present in the edit session's overlay renders its team
color at reduced strength until Save succeeds; read-only View Teams has no pending concept and
always renders full-strength.

**Revised after live testing — the faded color alone read as too subtle.** Two fixes, both from
direct user feedback on the running app:
- A small red corner flag (CSS border-triangle, top-left) now renders on top of the faded color
  for any pending row — a much harder-to-miss signal than a color-strength difference alone.
- `pendingPlayerIds` was overlay-*presence*-based, which broke badly for Generate specifically:
  Generate rebuilds the overlay from every player in its response, including ones whose seeded
  position didn't actually move, so a presence check flagged everyone as pending and the
  distinction disappeared entirely right when it mattered most. Fixed to compare each player's
  effective team against their last-***saved*** bucket instead — only players Generate (or a
  manual move) actually relocated show as pending now.

### 6. Default landing swap — Teams first, Game Details as a sheet

**Not in the original plan — added from live feedback after the screen was working.** The user
wanted tapping a game to land straight on Teams (something people do far more often than edit
time/location), with the old `GameViewPage` becoming a secondary destination — a swap already
flagged as a possible future move right after Stage 3 (see the now-resolved
`future_manage_teams_default_landing` memory). Concretely:

- `GameListItem` now links to `/games/:id/teams` instead of `/games/:id`.
- `GameTeamsPage` (both `ViewTeamsView` and `EditTeamsView`) now also takes `game` and
  `isOrganiser`, and renders `components/GameDetailsSheet.tsx` — a read-only summary
  (Location/Start Time/Duration/Team Size/Status/Winner) that pops up from the bottom, over the
  footer, rather than an inline accordion pushing the roster down (an inline version was
  considered and explicitly rejected in favor of this after the user pictured it more concretely
  mid-conversation). Its footer trigger sits where the old disabled "Invite" placeholder used to
  be: `Back | Game Details | Save`.
- **The sheet's "Manage Game" button — the only way to reach `GameViewPage` now — only renders for
  the organiser**, regardless of scheduled/finished (an organiser still wants Delete Game on a
  finished game, say). There's no read-only "View Game" variant at all: a non-organiser has
  nothing to do on that screen that the sheet doesn't already show them read-only, so the link
  simply isn't offered to them — `showManageLink` is exactly `isOrganiser`, not `canEdit`.
- The real "Invite Players" placeholder (still `disabled`, Stage 5) moved from `GameViewPage`'s
  footer-adjacent button stack onto the Teams screen instead, positioned above `AddNonUserPlayerForm`
  in `EditTeamsView`'s content — `GameViewPage` keeps its own copy too, since removing it wasn't
  asked for and it's harmless being disabled in both places.
- Back-navigation was rethought for the new hierarchy: `GameTeamsPage`'s `Back` now goes to `/`
  (the games list, since Teams is the entry point now), and `GameViewPage`'s `Back` now returns to
  `/games/:id/teams` (since Game View is reached from there now, not the list directly).
- **`GameViewPage`'s own "Manage Teams"/"View Teams" button moved into its footer**, positioned in
  the same middle slot as the Teams screen's "Game Details" button — a follow-up bit of feedback
  asking for visually symmetric navigation between the two screens. It was previously one of the
  stacked content buttons alongside Invite Players/Record Result/Delete Game.

### 7. Testing

`GameTeamsPage.test.tsx` (mirrors `GameViewPage.test.tsx`'s hook-mocking approach) covers: loading/
error states, read-only rendering for a non-organiser and for a finished game, a `<select>` move
staying pending (no Save call), Save sending the merged ids, Reset discarding a pending move,
Generate seeding from the last-saved split (and *not* marking unchanged seeded players as
pending — a direct regression test for the bug above), Remove from Game's modal-vs-immediate split
by tag/no-tag, Add Non-User Player's submit and inline field-error paths, the Game Details sheet's
content and its organiser-only Manage Game link, and both screens' Back destinations.
`TeamRosterRow.test.tsx` covers the current-state-aware option list directly. `GameViewPage.test.tsx`
and `GameListItem.test.tsx` were updated for the new navigation targets.

## Explicitly out of scope for this stage

- Invite Players — still `disabled`, Stage 5.
- Configurable Generate "competitiveness" (the `Differential` value) — fixed at `200` for v1, no
  UI control; flagged by the user as a future job, not Stage 4 scope.
- A click-outside-cancels behavior on the Add Non-User Player inline form (06-b's "clicking the
  main page discards it as though Cancel was clicked" annotation) — only an explicit Cancel button
  is wired; not worth the added complexity for v1.

## Verification

- `npm run build`, `npm run lint`, `npm run test -- --run` — all clean, 197 tests passing (up from
  171 at the end of Stage 3).
- Browser-verified what's checkable without a real Auth0 session (public landing loads clean,
  guarded routes redirect correctly rather than crashing) early on; the rest of this stage's
  verification — including all the live-feedback rounds that reshaped the pending styling and the
  navigation structure — was the user testing the running app directly with real credentials, not
  something this session could drive itself. Branch not pushed yet at the time of writing; the
  user is testing locally first.

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
  color strength until Save. **Superseded by the next two entries** — the strength difference alone
  turned out too subtle in practice, and the original presence-based `isPending` check was actually
  wrong, not just a simplification; both fixed after live testing.
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
- **Pending/saved distinction — live feedback, two rounds.** First round: "the difference is too
  subtle" (the color-strength fade alone), fixed by adding a red corner flag on top of it. Second
  round, more serious: "when you hit generate, we lose the pending/saved distinction" — root cause
  was `pendingPlayerIds` being presence-based (`Object.keys(overlay)`), and Generate's own success
  handler rebuilding the overlay from *every* player in its response, seeded-and-unchanged ones
  included. So after any Generate, literally every player in the game read as "pending," which is
  indistinguishable from the feature not existing at all. Fixed by making `isPending` a value
  comparison against each player's last-*saved* bucket instead of an overlay-presence check —
  correct for Generate, and also fixes the ping-pong edge case the original design had explicitly
  accepted as a simplification (a Home→Away→Home round-trip in one session no longer falsely shows
  pending either, now that it's judged by fact rather than by touch history).
- **Default landing swap: Teams first, Game Details as a bottom sheet.** See Approach section 6 for
  the mechanics. Went through several rounds of live refinement in one sitting: first framed as
  "print game details at the top of the page in a closed-by-default accordion," then "the Manage
  Game link should live in that expando, organiser-only," then "actually pop it from the bottom,
  where the Invite button currently sits — move the real Invite Players button up above Add
  Non-User Player instead," then a follow-up asking `GameViewPage`'s own reverse-navigation button
  to sit in the same footer position as "Game Details" for visual symmetry between the two screens.
  Each round was implemented and left for the user to react to live rather than re-confirmed in
  chat first — matched how the rest of this stage's live-feedback loop was already running.
