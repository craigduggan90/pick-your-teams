# Teams.Authoriser

Local-dev stand-in for the production Lambda authorizer described in the main repo's
`claude.md` (Auth model section). This is meant to become the real thing eventually — it's built
against the actual AWS Lambda authorizer request/response shape (`Amazon.Lambda.APIGatewayEvents`,
a REQUEST-type custom authorizer), not a hand-rolled contract, so nothing here needs reshaping
once real logic lands, only filling in.

## What it does today

`Teams.Authoriser/Function.cs` is the real Lambda handler. Right now it:

1. Reads the `Authorization` header off the incoming request.
2. Parses it as a JWT. Missing, not `Bearer <token>`, or not structurally a well-formed JWT →
   **Deny**.
3. Verifies the signature for real against Auth0's live JWKS (dev tenant
   `dev-e1zjkp6ynw1uag2f.us.auth0.com`): fetches the OIDC discovery document, finds the signing
   key matching the token's `kid`, builds a certificate from its `x5c`, and validates issuer,
   audience, signature, and expiry against it. Fetched fresh on every call — no caching, since
   there's little point caching inside a Lambda. Fails any of that → **Deny**.
4. **Valid, correctly-signed token → still Deny.** Turning a verified token into a resolved
   `Teams-User-Id`/`Tag`/`Name` triple needs a `Teams.Api` endpoint that doesn't exist yet
   (`GetByExternalIdAsync` exists on the repository but isn't exposed over HTTP — see the `TODO`
   in `Function.cs`). Until that lands, this component denies every request, which is the honest,
   fail-safe state to be in.

All of the real logic lives in `Teams.Authoriser/Auth/` as small, independently testable modules
(header/token parsing, JWKS fetch, key selection, signature validation, policy building).
`Function.cs` itself is a thin orchestrator and isn't unit tested — everything it calls into is.

## Projects

- **Teams.Authoriser** — the real Lambda function. Never run directly; a class library, not an
  executable. Redeployable to a real Lambda unchanged.
- **Teams.Authoriser.LocalHost** — the thing you actually run locally. A tiny hand-written
  ASP.NET Core app (`http://localhost:5210`) with one endpoint, `POST /authorize`: deserializes
  the HTTP body into the same request type API Gateway would build, calls
  `Function.FunctionHandler` with a `TestLambdaContext` (`Amazon.Lambda.TestUtilities` — a small
  dependency-free `ILambdaContext` POCO, not an emulation tool), serializes the response back.
  This is what `Teams.DevGateway` calls on every request. No SAM CLI, no LocalStack, no AWS CLI.
- **Teams.Authoriser.UnitTests** — xUnit v3 + NSubstitute, matching the main repo's convention.
  Covers every `Auth/` module, including real signature verification against a self-signed test
  certificate (no network calls).

## Running it

```bash
dotnet run --project Teams.Authoriser.LocalHost
```

## Manually debugging Function.cs

For interactive debugging — stepping through the JWKS/signature logic with a real captured
token — use AWS's own **Amazon.Lambda.TestTool** (installed as a local dotnet tool, see
`.config/dotnet-tools.json`):

```bash
dotnet tool run dotnet-lambda-test-tool-10.0 -- --path Teams.Authoriser
```

This opens a browser UI where you can paste a JSON payload (an `APIGatewayCustomAuthorizerRequest`
with a real `Authorization` header) and step through the function with a debugger attached. It's
a Blazor Server app with no REST endpoint of its own — that's why `Teams.Authoriser.LocalHost`
exists separately for `Teams.DevGateway`'s automated per-request calls.

## Upgrade path

When `Teams.Api` gets a `GetByExternalId` lookup (and create-if-missing), the TODO in
`Function.cs` becomes: look up `result.Subject` (the JWT `sub` claim), create the user if not
found, and return an **Allow** policy with the resolved `Teams-User-Id`/`Tag`/`Name` in the
response's context so `Teams.DevGateway` can turn them into headers.
