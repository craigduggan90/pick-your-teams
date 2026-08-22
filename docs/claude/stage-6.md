# Stage 6 — View Invites

## Context

Not one of `claude.md`'s original five stages — a feature the user proposed after Stage 5
shipped: a "View Invites" button on Game Details (reached via "Manage Game"), listing a game's
invitations and their status (Pending/Accepted/Declined/Failed). Same class of addition as
Stage 2's Logout or Stage 3's New Game screen — a real gap discovered live, not a rebuild of
anything.

Checking the API surfaced a real backend gap before any frontend work started: `GET /invitations`
already supported filtering by `gameId` (ownership-guarded to the game's organiser — exactly the
actor who'd see this button), but `InvitationModel`/`InvitationDetailModel` had no way to show
*who* each invitation was for — only `Organiser` (redundant on this screen, since the viewer
already knows it's their own game). User asked for this to be fixed as a Claude-authored backend
change rather than doing it themselves, unlike every prior stage's backend gaps.

## Approach

### 1. Backend change, its own branch

`backend/stage6-invitee-field`, branched off `main`, merged before this stage's frontend branch
existed — same sequencing Stage 4's three backend gaps used (landed as `main`'s PR #8 first).
Naming follows the existing `backend/stage4-prep-changes` precedent for a backend-only branch
separate from a frontend stage branch.

Added `InvitationInviteeModel` (`Id`/`Tag`/`DisplayName`, mirroring `InvitationOrganiserModel`'s
shape exactly) and wired an `Invitee` field onto both `InvitationModel` and `InvitationDetailModel`
in `InvitationsMapper.cs`, sourced from `invitation.User`. No repository or migration change
needed — `invitation.User` was already eager-loaded in both `ReadOnlyInvitationsRepository` query
paths, just never surfaced on a response model. Covered by mapper unit tests (populated and
null-invitee cases) and integration assertions on `GetInvitationById` and the `GetInvitations?
gameId=` path specifically (the exact call this stage's frontend makes). 1,045 backend tests green,
`dotnet format --verify-no-changes` clean.

### 2. Frontend branch

`stage-6-view-invites`, branched off `main` once the backend PR merged. Re-verified the merged
`InvitationModel`/`InvitationsMapper` source directly before building, rather than trusting the
backend branch's own description.

### 3. API layer (`src/ui/src/api/invitations.ts`)

`InvitationInviteeModel` type (same shape as the existing `InvitationOrganiserModel`, kept as a
distinct type to match the backend's own distinct type) and `invitee: InvitationInviteeModel |
null` added to `InvitationModel`. No new endpoint or hook needed — `useInvitations` (built in
Stage 5) already supports a `gameId` filter and cursor pagination; this screen is a second consumer
of the exact same hook with different params, not a new one.

### 4. Components

- `InvitationStatusBadge` (`components/InvitationStatusBadge.tsx`) — mirrors `GameStatusBadge`'s
  shape exactly (a `Record<Status, string>` label map, a `Record<Status, string>` style map, same
  pill markup). `Open` displays as "Pending" (the enum name reads as internal, not something an
  organiser would recognise); `Accepted` is Success-colored, `Failed` is Error-colored, `Declined`
  is neutral (Light Grey) rather than Error — declining is a normal outcome (the invitee just said
  no), not a problem, so it doesn't get the same visual weight as an actual delivery/validation
  failure.
- `GameInviteListItem` (`components/GameInviteListItem.tsx`) — the organiser-facing counterpart to
  Stage 5's `InvitationListItem`: invitee name/tag plus the status badge, no Accept/Decline (this
  is the organiser's own game, not an invitation belonging to the viewer).

### 5. Screen (`pages/GameInvitesPage.tsx`, route `/games/:id/invites`)

Same list-with-Load-More shape as `MyInvitationsPage`, but filtered by `gameId` instead of
`userId`, and read-only. Organiser-only, enforced the same way as `InvitePlayersPage`: once
`useGame`/`useSelf` resolve, a non-organiser is redirected to `/games/:id/teams` rather than left
on a page with nothing to show them — `GetInvitations`' `gameId` filter is already
ownership-guarded server-side, but a non-organiser could still land on the route directly by URL,
and without the redirect they'd be stuck on an indefinite loading state (the invitations query
never enables for them) rather than sent somewhere useful.

### 6. Wiring (`pages/GameViewPage.tsx`, `App.tsx`)

"View Invites" added to Game View's organiser-only action stack, above Record Result — shown for
any game status, not gated on `Scheduled` the way Record Result and Invite Players are, since
reviewing who was invited to a *finished* game is still useful, unlike acting on a scheduled one.
Route added to `App.tsx` behind `RequireAuthAndTag`, same as every other authenticated route.

### 7. Testing

New: `InvitationStatusBadge.test.tsx`, `GameInviteListItem.test.tsx`, `GameInvitesPage.test.tsx`
(loading/error/empty/populated/pagination/organiser-redirect/Back, mirroring
`MyInvitationsPage.test.tsx`'s structure). Updated: `GameViewPage.test.tsx` (View Invites present
for the organiser at any status, absent for a non-organiser, navigates correctly) and the two
Stage 5 test fixtures that construct `InvitationModel` object literals directly
(`InvitationListItem.test.tsx`, `MyInvitationsPage.test.tsx`) — needed an `invitee` field once the
type gained one.

## Explicitly out of scope for this stage

- No diagram exists for this screen at all — built directly from the user's description, not
  against any `docs/ui-design` reference.
- No date-sent timestamp on each row — `InvitationModel` (the list envelope's item type) doesn't
  carry `Created`/`Modified`, only `InvitationDetailModel` does. Not worth a per-row detail fetch
  just for a sent date; the status is the useful information here.
- No filtering/sorting controls on this screen (by status, by date, etc.) — the user's ask was
  "list the invites and their status," nothing about filtering it further. Easy to add later if
  wanted.

## Verification

- `npm run test -- --run` — 257 tests passing (up from 242 at the end of Stage 5).
- `npm run build` and `npm run lint` — clean, no new warnings.
- Backend: 1,045 tests passing across the whole solution, `dotnet format --verify-no-changes`
  clean.
- Browser-verified what's checkable without a real Auth0 session: the new
  `/games/:id/invites` route redirects to `/` rather than crashing when unauthenticated, no
  console errors. Seeing the actual invite list populated (and the organiser-redirect firing for a
  real non-organiser session) needs a human with real Auth0 credentials and a live backend, same
  as every prior stage.

## Decisions log

- **Claude makes the backend change, not the user.** Every prior stage's backend gaps (Stage 3's
  deferred location rule, Stage 4's three roster/tag fixes) were the user's own change. This one
  the user asked me to do directly — "just be careful and match the existing coding standards."
  Followed the closest existing precedent line-for-line: `InvitationOrganiserModel`'s exact shape
  and the same `?.ToModel()`-style mapping pattern already used for `Organiser`.
- **Backend change gets its own branch, off `main`, merged before the frontend branch starts.**
  Matches the existing `backend/stage4-prep-changes` naming/sequencing precedent rather than
  bundling the backend change into the frontend stage branch — keeps the backend PR reviewable on
  its own, and the frontend branch only ever builds against a merged, real API contract.
- **`Open` displays as "Pending."** Not a diagram decision (there's no diagram) — the raw enum
  name reads as an implementation detail on a status list meant for a human to skim.
- **Declined isn't styled as an error.** Considered reusing the Error token for both Declined and
  Failed since both are "not accepted" outcomes; rejected because they mean different things to an
  organiser — Declined is the invitee making a normal choice, Failed is something going wrong
  (delivery, or the invitee already being in the game via `Invitation.DispatchError`). Declined
  uses the neutral Light Grey token instead, keeping Error specifically for Failed.
- **View Invites shown regardless of game status.** Record Result and Invite Players are both
  `isOrganiser && isScheduled` (acting on a scheduled game only); View Invites is
  `isOrganiser` only, since reviewing past invitations for a finished game is still a legitimate
  thing to want, unlike sending a new invite or recording a result twice.
