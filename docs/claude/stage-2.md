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
  (`PATCH /api/v1/users/{id}`, body `{ Tag?, DisplayName?, Email?, Mobile? }`).

  **Casing is asymmetric and this applies to every future API module, not just users:** the API
  serializes *response* bodies in camelCase (ASP.NET's default JSON naming policy) even though
  the C# model properties are PascalCase — `UserDetailModel` is typed `id`/`tag`/`displayName`/
  etc. to match the real wire format, confirmed via curl against the running API. *Request*
  bodies stay PascalCase (`UpdateUserRequestModel`), since ASP.NET's model binding is
  case-insensitive on input and that's what matches the C# request DTO directly. The
  `ProblemDetails.errors` dict (422/400 validation failures) is keyed PascalCase too (`Tag`, not
  `tag`) — those keys come from FluentValidation's `PropertyName`/`nameof(...)`, not the JSON
  naming policy, so they don't follow the response-body rule. This asymmetry cost real debugging
  time (see Decisions log) — check the actual response shape via curl/Swagger before assuming
  either casing for a new endpoint.

### 6. Hooks (`src/ui/src/hooks/`)
- `useSelf.ts` — TanStack Query `useQuery` wrapping `getSelf()`, enabled only when
  `isAuthenticated`.
- `useUpdateTag.ts` — `useMutation` wrapping `updateUser()`, invalidates the self query on
  success. Typed `useMutation<void, ApiError, string>` so `mutation.error` is a real `ApiError`
  in the common case — components still need an `instanceof ApiError` guard, since a token
  failure (e.g. `getAccessTokenSilently()` rejecting) can still surface a plain `Error`.
- No separate `useAccessToken` hook — planned initially, dropped as an unnecessary layer since
  `useAuth0().getAccessTokenSilently` is called directly in the one or two places that need it.

### 7. Routing / route guards
Per `01-login-and-registration.png`, unauthenticated users land on a **public "Team Picker"
screen** with Log In / Register buttons — not an automatic bounce to Auth0's hosted page — so this
isn't `withAuthenticationRequired`'s default behavior. Structure:
- `/` — public. Renders the Team Picker landing (Log In → `loginWithRedirect()`, Register →
  `loginWithRedirect({ authorizationParams: { screen_hint: 'signup' } })`). If already
  authenticated, redirects onward (through the same tag-check as below) instead of showing the
  landing again.
- `RequireAuth` — redirects to `/` if not authenticated (no auto Auth0 redirect); renders children
  once authenticated. Used to wrap the change-tag route.
- `RequireAuthAndTag` — `RequireAuth`, plus fires `useSelf`, redirects to `/change-tag` if
  `Id === Tag`. Nothing consumes this yet (Stage 3 adds the first real protected screen), but the
  component is built now so Stage 3 just wraps its routes in it.
- `/change-tag` — wrapped in `RequireAuth` only (must be logged in, but must NOT tag-redirect-loop
  on itself). **This is the one URL for both first-time setup and later changes** — `ChangeTagPage`
  checks `Id === Tag` itself and picks the `ChangeTag` component's mode accordingly: gate mode (no
  Back, blank field, header title "Set Your Tag") if the user still needs to set a tag, normal
  mode (Back enabled, prefilled with the current tag, header title "Change Tag") if they already
  have one and navigated here voluntarily. Stage 3's "Change Tag" button just needs to link to
  `/change-tag`; there's no second route or page to build for it.
  - **Returns the user to wherever they came from, not always `/`.** Whoever navigates *to*
    `/change-tag` — `TagGate`'s automatic redirect, or Stage 3's future Change Tag button — passes
    `state: { from: location.pathname }` (the shared shape lives in `lib/navigation.ts` as
    `ChangeTagLocationState`, so both sides use the same contract). On success or cancel,
    `ChangeTagPage` navigates back to `location.state?.from ?? '/'`. This is what makes gate-mode
    users land back on whichever page they were originally trying to reach (not just the Team
    Picker root), and will do the same for Stage 3's My Account once it exists — no extra wiring
    needed there beyond passing `state` when it links here.
- `/dev/components` — stays public/unguarded, as before.
- `/account` — added out of sequence, ahead of Stage 3, purely so there's a real page to click
  "Change Tag" from while testing (needed to be able to get back to `/change-tag` after the first
  gate pass, without it there's no in-app way to reach it again). `MyAccountPage` is a one-line
  placeholder plus a "Change Tag" button wired exactly like Stage 3's real one will be — navigates
  to `/change-tag` with `state: { from: '/account' }`. Wrapped in `RequireAuthAndTag` (not just
  `RequireAuth`), since viewing My Account presupposes you already have a tag. `HomePlaceholder`
  (rendered at `/` once tagged) links here too, so the whole loop is clickable without typing a
  URL: Home → My Account → Change Tag → back to My Account.

### 7a. Shared header title (`usePageTitle`)
Not in the original plan — added once the change-tag screen was built and it became clear that
having the screen render its own "Set Your Tag" bar produced two stacked header bars (the app's
persistent `Header` plus the screen's own), which doesn't fit the mobile-first single-header
layout. Instead:
- `hooks/usePageTitle.tsx` exports a `PageTitleProvider` (wraps the whole app, holds the current
  title in state) and two hooks: `usePageTitle(title)` — call once per routed page component,
  sets the shared title for as long as that page is mounted — and `useHeaderTitle()`, read once
  by `App.tsx`'s shell to feed the persistent `Header`.
- **Every routed page must call `usePageTitle`,** including ones that just want the default app
  name (`TeamPickerPage` calls `usePageTitle(APP_NAME)`) — there's no automatic reset when
  navigating away from a page that set a custom title, so a page that skips this would show
  whatever the previous page left behind.
- `lib/constants.ts` now holds `APP_NAME` (`"Pick Your Teams"`), deduplicated out of `Header`'s
  default, `Footer`, and `TeamPickerPage`.

This pattern is the one future stages should follow for every new screen's header title, not a
Stage 2-only concern.

### 8. Change-tag component (`ChangeTag`, in `components/ChangeTag.tsx`)
Dual-use per `claude.md`: a `mode: 'gate' | 'normal'` prop. Originally built and named `TagSetup`
for gate-only use; renamed once it became the shared entry point for both first-time setup and
later changes — "tag-setup" stopped being an accurate name for something also used to change an
existing tag (see Decisions log).
- Both modes: `TextInput` (Stage 1 primitive, floating label) for the tag, live requirements list
  driven by the **real API validation rules** (found in `UpdateUserCommandValidator.cs` /
  `Constants.TagRegexPattern`): 3–36 characters, must start with a letter/digit/underscore, only
  letters/digits/`.`/`_`/`-` after that, must contain at least one alphanumeric character.
- Save feedback uses the Stage 1 **Toast primitive**, not the diagram's inline saving/error/
  success banners — the diagram was built before Stage 1 had a toast system to reuse, and once it
  existed, duplicating a second feedback mechanism inline didn't make sense. `toast.success`/
  `toast.error` fire off the mutation's state; only the *field-level* error (e.g. "Tag not
  available.") stays inline under the `TextInput`, matching the diagram's "Field error (if we
  have it)" annotation specifically. Because `Toaster` is mounted globally in `App.tsx` (outside
  routed content), the success toast survives the redirect, so the artificial "show success for a
  beat before navigating" delay the diagram implies isn't needed — `onSuccess` fires immediately.
  **This sets the precedent for later screens with similar diagram banners** (e.g.
  `04-view-game.png`'s "Changes Saved!") — prefer the toast over a bespoke inline banner unless
  there's a specific reason not to.
- Gate mode: no Back/Cancel button, "Not Now" omitted entirely (per `claude.md`'s resolved
  decision).
- Normal mode: Back/Cancel enabled, prefilled with the current tag. Wired to `/change-tag` now
  (see routing above) — Stage 3's real My Account "Change Tag" button just links there, same as
  the minimal placeholder below already does.
- Both modes call the same `onSuccess`/`onCancel` props, which `ChangeTagPage` wires to "go back
  to wherever the user came from" (see routing above) — the component itself doesn't know or care
  where that is.

### 9. Testing
Vitest + RTL as established in Stage 1. Component/hook tests mock the `api/users.ts` module
directly (`vi.mock`) rather than hitting a real backend or introducing MSW — keeps the dependency
footprint the same as Stage 1. Covers: route guard redirect behavior, the change-tag component's
states in both modes, requirements list reflecting the real validation rules, mutation error
surfacing (duplicate-tag "Tag not available." message), and `ChangeTagPage`'s mode selection and
return-navigation (with `ChangeTag` itself mocked out, so the page's own responsibility is tested
in isolation).

## Explicitly out of scope for this stage
- Games List / My Account / any Stage 3+ screens.
- Playwright/E2E.
- Any AWS infrastructure (API Gateway, Lambda authorizer) — local dev shim only.
- Modifying `src/api` in any way (no CORS changes) — frontend-only, per `claude.md`.

## Verification
- `npm run build` and `npm run test -- --run` both green.
- Manual browser check: log in via real Auth0 → lands on Team Picker if not authenticated, or
  redirects to `/change-tag` if the seeded dev user's `Tag === Id`, or through to `/` otherwise.
  Submit a tag, confirm success + redirect back to `/`, confirm a duplicate/invalid tag shows the
  right error.

## Decisions log
Resolved during implementation, kept here for traceability — outcomes are already reflected
inline above.

- **Local dev API access** — the API expects raw `Teams-User-*` headers and there's no deployed
  API Gateway/authorizer to produce them locally; real Auth0 login can't authenticate local API
  calls on its own. → Vite dev proxy injects a fixed dev user's headers (see "Vite dev proxy"
  above); Auth0 login UX itself is still real. Confirmed acceptable to develop against this
  "rigged" backend for now; tests mock the API client rather than needing MSW or a real backend.
- **Auth0 "Allowed Web Origins"** — after the seeded dev user was provided, testing hit silent
  re-authentication failures (`/authorize?...&prompt=none` returning 400) on page refresh, because
  `cacheLocation: "memory"` means every reload needs a silent iframe-based token renewal, and that
  requires the app's origin to be listed in the Auth0 application's **Allowed Web Origins** — a
  field separate from Allowed Callback URLs, easy to miss. Adding `http://localhost:5173` there
  resolved it. Worth checking first if this recurs against a different Auth0 tenant/environment.
- **Response casing bug** — `UserDetailModel` was typed PascalCase to match the C# source, but the
  API actually serializes responses camelCase. Every field read (`.Id`, `.Tag`, ...) was silently
  `undefined`, which incidentally still "worked" for the gate redirect (`undefined === undefined`
  is `true`, so untagged-looking behavior happened to be right) but broke the tag-save mutation
  outright, since it never had a real user id to PATCH and failed before any network request —
  the kind of bug that's easy to miss because the visible symptom (stuck on the gate) looks
  correct. Fixed by correcting `UserDetailModel` to camelCase and auditing every `.Id`/`.Tag`
  field read; request bodies and `ProblemDetails.errors` keys were confirmed unaffected (see the
  API client section above). Tests were also fixed — they'd used the same wrong casing as the
  type, so they passed despite the bug and wouldn't have caught a regression here.
- **Toast vs. inline banners for save feedback** — see the change-tag component section above.
- **"tag-setup" → "change-tag" rename, plus return-navigation** — once the component became
  genuinely dual-use (gate *and* normal mode reachable at the same URL), "tag-setup" stopped
  describing what it does for an already-tagged user changing their tag. Renamed throughout:
  `TagSetup`/`TagSetupProps`/`TagSetupMode` → `ChangeTag`/`ChangeTagProps`/`ChangeTagMode`,
  `TagSetupPage` → `ChangeTagPage`, route `/tag-setup` → `/change-tag`. Separately, the original
  "always navigate to `/` when done" behavior was wrong for two real cases: a user gated to
  `/change-tag` from somewhere other than `/` should land back on *that* page, not always root;
  and Stage 3's Change Tag button (from My Account) needs to return to My Account specifically,
  not root. Fixed with a `state: { from: <pathname> }` convention (`ChangeTagLocationState` in
  `lib/navigation.ts`) — whoever navigates to `/change-tag` supplies it, `ChangeTagPage` reads it
  back on success/cancel, defaulting to `/` only if it's missing (e.g. someone bookmarks the URL
  directly).
- **Primary/Secondary colors — several rounds, current values are explicitly temporary.** First
  real values given were Primary `#00B295` / Secondary `#F05D23`; white text on `#00B295` computed
  to ~2.70:1 contrast (under WCAG AA's 3:1 floor even for large text/UI components), so Primary
  was darkened to `#32936F` (~3.79:1). That was superseded before landing — current values are
  Primary `#465775` (blue) / Secondary `#EF6F6C` (coral), explicitly called out as **not final,
  just something to build against for now** (Secondary is only ever used as an accent, Primary
  only for buttons/the header, so contrast tuning wasn't revisited for these). `:root`'s comment
  in `index.css` reflects this — don't assume the current hex values carry any contrast
  reasoning forward; re-check if/when these are finalized for real.
- **Real branding assets added, also temporary** — favicon (`favicon.ico`, `apple-touch-icon.png`,
  `icon-192`/`icon-512` + maskable variants, all in `src/ui/public/`, generated via IconKitchen)
  replaced the Vite placeholder `favicon.svg`. `Header`'s home button now shows the round
  `icon-192.png` instead of a generic placeholder dot. Not final branding, just something to use
  for now — same spirit as the temporary colors above.
