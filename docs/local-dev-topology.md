# Local dev topology

Mirrors the production shape in `docs/arch-design/aws-design.png` minus AWS itself: the UI logs
in via real Auth0, and every request runs through a real reverse proxy and a real (if not yet
fully implemented) authorizer before reaching the real, unmodified `Teams.Api`.

```
UI (Vite dev server, :5173)
  -> Teams.DevGateway (:5200)   [src/gateway  — reverse proxy, plays API Gateway's role]
       -> Teams.Authoriser.LocalHost (:5210)   [src/authoriser — plays the Lambda authorizer's role]
       -> Teams.Api (:5199)   [src/api — real, unmodified]
```

`Teams.DevGateway` and `Teams.Authoriser` are their own solutions under `src/gateway` and
`src/authoriser` — not part of `Teams.sln`, not touched by the existing CI workflows. See each
project's own `README.md` for what it does and why.

## Current state: everything denies

`Teams.Authoriser` validates real Auth0 tokens for real (JWKS signature check against the dev
tenant) but always returns Deny — turning a verified token into a resolved user needs a
`Teams.Api` endpoint that doesn't exist yet (`GetByExternalId`, see the `TODO` in
`src/authoriser/Teams.Authoriser/Function.cs`). Until that lands, logging into the app through
this chain will 401 at `Teams.DevGateway` every time — that's the honest, fail-safe state, not a
bug. This still proves the whole pipeline is wired correctly, and API-side work can still use the
existing bypass: hit `Teams.Api` directly with `Teams-User-*` headers set manually (same trust
boundary the E2E test suite already uses).

## Running it

One-time setup:

```bash
cd src/authoriser && dotnet build
cd src/gateway && dotnet build
```

Then, from the repo root:

```bash
npm run dev:all
```

Starts all four processes (`Teams.Api`, `Teams.Authoriser.LocalHost`, `Teams.DevGateway`, the UI
dev server) with labeled, merged console output via `concurrently`. `Ctrl+C` stops all four.

To run them individually instead:

```bash
dotnet run --project src/api/Teams.Api                              # :5199
dotnet run --project src/authoriser/Teams.Authoriser.LocalHost      # :5210
dotnet run --project src/gateway/Teams.DevGateway                   # :5200
npm run dev --prefix src/ui                                         # :5173, proxies /api -> :5200
```

## Switching the UI's target

`npm run dev` (in `src/ui`) points the Vite dev proxy at `Teams.DevGateway`
(`.env.development`). `npm run prod` runs the same dev server in production mode
(`.env.production`) instead — a stand-in for pointing at the real AWS API Gateway once one's
deployed; until then it points straight at `Teams.Api`, so protected calls will 401 (nothing sets
the `Teams-User-*` headers on that path), but unauthenticated behaviour can be sanity-checked and
the dev/prod switch itself is proven to work.

## Manually debugging Teams.Authoriser

`src/authoriser`'s `README.md` covers using `Amazon.Lambda.TestTool` to step through
`Function.cs` with a debugger attached and a real captured token, separately from
`Teams.Authoriser.LocalHost` (which is what `Teams.DevGateway` actually calls automatically).
