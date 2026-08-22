# Stage 5 — Invitations

## Context

Stage 4 (`stage-4-teams-management`, merged to `main`) built the Teams screen with a disabled
"Invite Players" placeholder, waiting on this stage, and `GameViewPage` had a matching disabled
copy of the same button. Stage 5 is `claude.md`'s final stage: Create Invitations (tag-only, per
the "build against this, not the stale diagram" contract) and My Invitations (accept/decline).

A prep session (recorded in memory as `stage5_kickoff_decisions`) read `claude.md`,
`docs/claude/stage-1.md`–`stage-4.md`, both invitation diagrams, and cross-checked the actual
Invitations API source ahead of this branch, resolving the open questions before any code was
written:

- No backend gap on `InvitationOrganiserModel` — already present on both `InvitationModel`/
  `InvitationDetailModel`, same pattern as Stage 3's `GameOrganiserModel`. List envelope is
  `{ data, cursor, count }`, identical to Games.
- Invite Players is reachable only from the Teams screen (`EditTeamsView`), organiser-only — the
  `GameViewPage` copy was to be removed entirely, not left disabled.
- My Invitations nav (Header icon, route to `/invitations`) was already built in a prior session
  ahead of this stage, with a `TEMPORARY` hover-swap placeholder for the pending-icon badge,
  explicitly waiting on this stage to wire up the real `GET /users/self` `pendingInvitations`
  field (already live on the backend).
- Accept can 422 two ways — invitation already resolved to the opposite outcome, and (new since
  Stage 4's roster-cap change) game already at capacity — both need surfacing.
- Confirmation UI on this stage should use the `Sheet` primitive, not the removed `Modal` — this
  stage ended up not needing any confirmation sheet at all (see Approach), so this was moot in
  practice.

This session re-verified the prep session's findings directly against the current `InvitationsController`/
`InvitationsMapper`/command handlers/validators rather than trusting the memory summary, and
confirmed `feat/bottom-sheet-modals` was already merged to `main` before branching.

## Approach

### 1. Branch

`stage-5-invitations`, branched off the latest `main`.

### 2. API layer (`src/ui/src/api/invitations.ts`)

`InvitationGameModel`/`InvitationOrganiserModel`/`InvitationModel`/`InvitationsPage` types
(camelCase, matching the wire format confirmed against `InvitationsMapper.cs`), `getInvitations`/
`createInvitations`/`acceptInvitation`/`declineInvitation`. Query params for `getInvitations` are
built PascalCase, same convention as `api/games.ts`. `api/users.ts`'s `UserDetailModel` gained
`pendingInvitations: number` — the field already existed on the backend (added ahead of this
stage) but the frontend type hadn't picked it up yet.

`acceptInvitation`/`declineInvitation` don't need a request body — `POST`/`DELETE`
`/v1/invitations/{id}` respectively, matching `InvitationsController`.

### 3. Hooks (`src/ui/src/hooks/`)

`useInvitations` (`useInfiniteQuery`, cursor-paginated, mirrors `useGames`; takes an optional
`{ enabled }` so callers can defer fetching until a dependency like the current user id is known),
`useCreateInvitations`, `useAcceptInvitation`, `useDeclineInvitation`. The latter two invalidate
both `['invitations']` (the list) and `['self']` (the header badge count) on success — accepting
or declining changes both.

### 4. Invite Players screen (`pages/InvitePlayersPage.tsx`, route `/games/:id/invite`)

No current diagram exists for this screen — `05-invite-players.png` is stale (the mixed tag-or-
email design with per-row claim tracking it shows was deleted from the API entirely before this
stage). Built directly against `claude.md`'s tag-only contract instead: a dynamic list of Tag
`TextInput` rows (start with one, "+ Add Another Tag" to grow, a remove button per row once
there's more than one), "Send Invitations" disabled until at least one non-blank tag exists,
`POST /invitations { GameId, UserTags[] }` on submit with only the trimmed non-blank tags.

- Organiser-only, enforced client-side: once `useGame`/`useSelf` resolve, a non-organiser is
  redirected back to `/games/:id/teams` rather than shown a form with nothing to do (unlike
  `GameViewPage`, which still has a real read-only mode for non-organisers).
- Errors: `CreateInvitationsCommandValidator`'s failures all carry a field-level `errors` dict
  (FluentValidation), including the "Tag not found: {tag}" ones — those use an empty-string
  `PropertyName`, so the dict key is `""`. Rendering `Object.values(errors).flat()` as a flat list
  handles this correctly without needing to special-case the empty key — matches `claude.md`'s "no
  need to match errors back to specific input rows by index." A generic toast covers the rare case
  of an error with no `errors` dict at all (e.g. a 403/404).
- Success toasts and navigates back to `/games/:id/teams`.

### 5. My Invitations screen (`pages/MyInvitationsPage.tsx`, route `/invitations`)

`useInvitations({ userId: self.id, status: 'Open' })`, gated on `self` having loaded first (the
ownership guard on `GetInvitations` requires the actor to *be* the `userId` being filtered by).
Each row (`components/InvitationListItem.tsx`) shows the formatted date/time, `Location |
Organised by @tag`, and Accept/Decline icon buttons per `07-my-invitations.png`.

- **Per-row pending state without extra component state.** `useAcceptInvitation`/
  `useDeclineInvitation` are each called once for the whole page (not per row), so "is this
  specific row's request in flight" is derived as `mutation.isPending && mutation.variables ===
  invitation.id` rather than tracked separately — the mutation's own `variables` already carries
  which invitation id it was called with.
- The diagram's "working" dimmed transitional row state and the row visibly disappearing on
  success are both out of scope per `claude.md` — a plain disabled state on both buttons during
  either request is what's built. A successful accept/decline just lets the list refetch (Open-
  only, so the resolved invitation naturally drops out) rather than animating anything.
- Both 422 cases (`RequestHandlerException`, not FluentValidation — no `errors` dict, just
  `problem.detail`) surface via `toast.error(error.problem.detail ?? error.message)`, matching
  `GameViewPage`'s existing error-toast pattern, not `GameTeamsPage`'s errors-dict-flattening one
  (there's no field-level structure to flatten here).

### 6. Wiring up the two disabled placeholders from Stage 3/4

- `EditTeamsView`'s "Invite Players" button (`pages/GameTeamsPage.tsx`) — `disabled` replaced with
  `onClick={() => navigate('/games/:id/invite')}`. Still implicitly organiser-only, since
  `EditTeamsView` itself only renders for `canEdit`.
- `GameViewPage.tsx`'s disabled "Invite Players" block — removed entirely, per the kickoff
  decision (Teams is the only entry point now; a second disabled copy on the secondary screen was
  never load-bearing).

### 7. Header badge (`components/Header.tsx`)

Replaced the `TEMPORARY` hover-swap comment/markup (a `:hover`-triggered opacity crossfade between
`invitations.png` and `invitations-pending.png`, explicitly left as a preview stand-in by the
session that built the nav) with the real trigger: `useSelf().data.pendingInvitations > 0` picks
which icon renders. `Header` already called other auth-aware hooks directly (`useAuth0`), so
calling `useSelf` directly here follows the same pattern rather than threading the count down as a
prop.

## Explicitly out of scope for this stage

- Viewing a single invitation via a direct email link (`GET /invitations/{id}`) — known gap in
  `claude.md`, no screen designed for it yet.
- The "working" transitional row state and the row-disappears-on-success animation on
  `07-my-invitations.png` — nice-to-have only per `claude.md`; a disabled-buttons loading state is
  enough for v1.
- Any client-side guard against inviting a tag who's already a player, already has an Open
  invitation to the same game, or inviting yourself — the backend doesn't validate these either
  (confirmed in the prep session); left as-is, not a new gap introduced by this stage.

## Verification

- `npm run test -- --run` — 241 tests passing (up from 197 at the end of Stage 4).
- `npm run build` and `npm run lint` — clean, no new warnings.
- Browser-verified what's checkable without a real Auth0 session: public landing loads clean with
  no console errors, and both new guarded routes (`/invitations`, `/games/:id/invite`) redirect to
  `/` rather than crashing when unauthenticated. The authenticated flow — sending an invitation,
  seeing the Header badge light up, accepting/declining from My Invitations — needs a human with
  real Auth0 credentials and a live backend to exercise end to end, same as every prior stage.

## Decisions log

Resolved during prep and implementation, kept here for traceability — outcomes are already
reflected inline above.

- **Invite Players entry point.** Confirmed in prep, executed here: `GameViewPage`'s disabled copy
  removed entirely rather than wired up or left disabled; Teams screen is the only entry point.
- **Dynamic tag-row form, not a diagram to build from.** `05-invite-players.png` is stale (the
  email/tag mixed design it shows was deleted from the API). No replacement diagram exists, so the
  form shape (growable list of Tag inputs, flat error list at the bottom) was designed directly
  against `claude.md`'s API contract description rather than any visual reference.
- **Flattening `errors` regardless of key, including the empty-string key.** Confirmed against
  `CreateInvitationsCommandHandler` source: the "Tag not found" failures use
  `ValidationFailure(string.Empty, ...)`, so their dict key is `""` once grouped by `PropertyName`
  in `ValidationExceptionHandler`. `Object.values(errors).flat()` handles this for free — no
  special-casing needed, and it's what `claude.md` already asked for.
- **Per-row pending state derived from `mutation.variables`, not separate row state.** Both
  Accept/Decline mutations are single hook instances shared across the whole list; rather than
  tracking a `pendingId` in component state, each row computes its own pending flag as
  `mutation.isPending && mutation.variables === invitation.id` — the mutation already knows which
  id it was last called with.
- **Header badge — real trigger wired up, replacing the temporary hover preview.** The Header
  icon/route/badge scaffolding was already built ahead of this stage with a `:hover`-triggered
  crossfade explicitly marked temporary; this stage's only Header change was swapping that trigger
  for `useSelf().data.pendingInvitations > 0`, confirmed already live on `GET /users/self`.
