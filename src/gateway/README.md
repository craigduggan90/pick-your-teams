# Teams.DevGateway

**Local dev only — never deploy this.** It's a plain reverse proxy that plays the role AWS API
Gateway plays in production (see the main repo's `docs/arch-design/aws-design.png`), for local
development only. It lives outside `Teams.sln` entirely — the same structural guarantee
`Teams.Api.EndToEndTests` has, just stronger here since this is a separate repo.

## What it does

No auth logic of its own. On every request:

1. Reads the `Authorization` header.
2. Builds the same request shape API Gateway would send a REQUEST-type custom authorizer, and
   `POST`s it to `Teams.Authoriser.LocalHost` (`http://localhost:5210`).
3. `Deny` → returns `401` immediately. `Teams.Api` never sees the request.
4. `Allow` (not reachable yet — `Teams.Authoriser` always denies until its user-resolution TODO
   is filled in) → will translate the resolved user out of the response context into
   `Teams-User-Id`/`Tag`/`Name` headers and forward through to `Teams.Api`. See the `TODO` in
   `Authorisation/AuthorisationHandler.cs`.

Built with ASP.NET Core minimal API + YARP (`Yarp.ReverseProxy`). The actual logic — building the
authorizer request, calling it, interpreting Allow/Deny — lives in `Authorisation/`, kept small
and readable rather than unit tested; this is a dev tool, not a shipped component.
`Program.cs` itself is thin: it wires the middleware and YARP together.

## Config

`appsettings.json`:

- `Authoriser:BaseUrl` — where `Teams.Authoriser.LocalHost` is running (`http://localhost:5210`).
- `ReverseProxy:Clusters:teams-api` — where the real `Teams.Api` is running
  (`http://localhost:5199`).

## Running it

```bash
dotnet run --project Teams.DevGateway
```

Listens on `http://localhost:5200`.
