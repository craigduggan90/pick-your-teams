# Stage 2 — Auth + Tag Setup

## Context

Stage 1 (branch `stage-1-foundations`, not yet merged to `main`) delivered the Tailwind/shadcn
foundations and tested primitives (Button, TextInput, Modal, Select, Toast, Header/Footer) with
no real screens. Stage 2 is the next step in `claude.md`'s five-stage build order: wire
`auth0-react`, add the route guard, integrate `GET /users/self`, and build the tag-setup screen
in its dual-use (gate/normal) mode so Stage 3 can reuse it for "Change Tag" without a rebuild.

Two things surfaced during research that aren't covered by `claude.md` and needed resolving
before this could be planned concretely:

1. **The API doesn't take bearer tokens.** It reads three raw headers
   (`Teams-User-Id`/`-Tag`/`-Name`) per request, with zero `Authorization` handling anywhere in
   `src/api`. In production these are injected by an AWS API Gateway custom Lambda authorizer
   (confirmed in `docs/arch-design/aws-design.png`) that doesn't exist as deployable code in this
   repo and isn't deployed anywhere. There's also no CORS configuration on the API. A real Auth0
   login calling the local API directly would simply fail.
2. **Resolved with the user:** develop against a "rigged" backend for now — a Vite dev-server
   proxy injects a fixed dev user's `Teams-User-*` headers for local API calls (frontend-only,
   mirrors the trust boundary `claude.md` already sanctions for E2E tests: *"hit the API directly
   with the `Teams-User-*` headers set manually"*). Real Auth0 PKCE login still runs for the
   actual UX; it just isn't what authenticates local API calls. Component/unit tests mock the API
   client rather than hitting anything real. Playwright/E2E stays out of scope for this stage —
   there's nothing genuine to E2E-test against yet either, same reasoning as Stage 1.

## Approach

### 1. Branch
`stage-2-auth-tag-setup`, branched off **`stage-1-foundations`** (not `main` — `main` doesn't have
Stage 1's scaffold yet, and Stage 2 needs it). This is a deviation from `claude.md`'s "branch off
latest main" workflow line; once Stage 1 merges, this PR's base can be retargeted.

### 2. Env config
`src/ui/.env.local` (gitignored, already covered by the scaffolded `*.local` rule) with:
- `VITE_AUTH0_DOMAIN`, `VITE_AUTH0_CLIENT_ID`, `VITE_AUTH0_AUDIENCE`
- `VITE_API_BASE_URL=/api` (routed through the dev proxy below)
- `VITE_DEV_USER_ID` / `VITE_DEV_USER_TAG` / `VITE_DEV_USER_NAME` — dev-only, used solely by the
  Vite proxy shim, never read by app code

Plus a committed `src/ui/.env.example` documenting all six with empty/placeholder values.

### 3. Vite dev proxy (local-only auth shim)
`vite.config.ts`'s `server.proxy['/api']` → `http://localhost:5199`, with a `configure` hook that
sets `Teams-User-Id`/`Teams-User-Tag`/`Teams-User-Name` on the proxied request from the
`VITE_DEV_USER_*` env vars. Only affects `vite dev`; production builds are unaffected since
there's no dev server at build/serve time.

**Requires a one-time manual step outside this repo:** a matching user row must already exist in
the local `Teams.db` with `Id`/`Tag`/`Name` equal to the `VITE_DEV_USER_*` values, or `GET
/users/self` 404s. Not something the frontend repo can seed.

### 4. `auth0-react`
`Auth0Provider` in `App.tsx` (wrapping the existing `QueryClientProvider`/`BrowserRouter`), PKCE
default, `cacheLocation: "memory"` (explicit, matching `claude.md`'s "in-memory access token"),
`domain`/`clientId`/`audience` from the env vars above, `redirectUri` = `window.location.origin`.

### 5. API client (`src/ui/src/api/`)
- `client.ts` — thin `fetch` wrapper: base URL from `VITE_API_BASE_URL`, always attaches
  `Authorization: Bearer <token>` (real production-shaped behavior; the dev proxy substituting
  headers is a transport concern the client code doesn't know about).
- `users.ts` — `getSelf()` (`GET /api/v1/users/self` → `UserDetailModel`), `updateUser(id, body)`
  (`PATCH /api/v1/users/{id}`, body `{ tag?, displayName?, email?, mobile? }`). Types mirror the
  API's `UserDetailModel`/`UpdateUserRequestModel` exactly (`Id`, `Tag`, `DisplayName`, `Rating`,
  `Email`, `Mobile: string | null`, `Created`, `Modified`).

### 6. Hooks (`src/ui/src/hooks/`)
- `useAccessToken.ts` — thin wrapper over `useAuth0().getAccessTokenSilently`.
- `useSelf.ts` — TanStack Query `useQuery` wrapping `getSelf()`, enabled only when
  `isAuthenticated`.
- `useUpdateTag.ts` — `useMutation` wrapping `updateUser()`, invalidates the self query on
  success.

### 7. Routing / route guards
Per `01-login-and-registration.png`, unauthenticated users land on a **public "Team Picker"
screen** with Log In / Register buttons — not an automatic bounce to Auth0's hosted page — so this
isn't `withAuthenticationRequired`'s default behavior. Structure:
- `/` — public. Renders the Team Picker landing (Log In → `loginWithRedirect()`, Register →
  `loginWithRedirect({ authorizationParams: { screen_hint: 'signup' } })`). If already
  authenticated, redirects onward (through the same tag-check as below) instead of showing the
  landing again.
- `RequireAuth` — redirects to `/` if not authenticated (no auto Auth0 redirect); renders children
  once authenticated. Used to wrap the tag-setup route.
- `RequireAuthAndTag` — `RequireAuth`, plus fires `useSelf`, redirects to `/tag-setup` if
  `Id === Tag`. Nothing consumes this yet (Stage 3 adds the first real protected screen), but the
  component is built now so Stage 3 just wraps its routes in it.
- `/tag-setup` — wrapped in `RequireAuth` only (must be logged in, but must NOT tag-redirect-loop
  on itself). Renders the tag-setup component in **gate mode**: no Back/Cancel, on success
  redirects to `/`.
- `/dev/components` — stays public/unguarded, as before.

### 8. Tag-setup component
Dual-use per `claude.md`: a `mode: 'gate' | 'normal'` prop.
- Both modes: `TextInput` (Stage 1 primitive, floating label) for the tag, live requirements list
  driven by the **real API validation rules** (found in `UpdateUserCommandValidator.cs` /
  `Constants.TagRegexPattern`): 3–36 characters, must start with a letter/digit/underscore, only
  letters/digits/`.`/`_`/`-` after that, must contain at least one alphanumeric character. Four
  states: initial / loading (submitting) / error (inline field error, e.g. server-side "Tag not
  available.") / success.
- Gate mode: no Back/Cancel button, "Not Now" omitted entirely (per `claude.md`'s resolved
  decision), on success navigates to `/`.
- Normal mode: Back/Cancel enabled, no forced redirect — not wired to a route yet (Stage 3's My
  Account "Change Tag" button does that), just needs to exist and work standalone for Stage 3 to
  consume without changes.

### 9. Testing
Vitest + RTL as established in Stage 1. Component/hook tests mock the `api/users.ts` module
directly (`vi.mock`) rather than hitting a real backend or introducing MSW — keeps the dependency
footprint the same as Stage 1. Covers: route guard redirect behavior, tag-setup's four states in
both modes, requirements list reflecting the real validation rules, mutation error surfacing
(duplicate-tag "Tag not available." message).

## Explicitly out of scope for this stage
- Games List / My Account / any Stage 3+ screens.
- Playwright/E2E.
- Any AWS infrastructure (API Gateway, Lambda authorizer) — local dev shim only.
- Modifying `src/api` in any way (no CORS changes) — frontend-only, per `claude.md`.

## Verification
- `npm run build` and `npm run test -- --run` both green.
- Manual browser check: log in via real Auth0 → lands on Team Picker if not authenticated, or
  redirects to `/tag-setup` if the seeded dev user's `Tag === Id`, or through to `/` otherwise.
  Submit a tag, confirm success + redirect, confirm a duplicate/invalid tag shows the right error.
