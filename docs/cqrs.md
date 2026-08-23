# CQRS / the home-rolled mediator

`Teams.Core` (`src/api/Teams.Core/CQRS`) has its own tiny mediator instead of a dependency on
MediatR or similar — small enough to read end to end in a few minutes, and it's exactly the same
shape every request/response library in this space converges on.

## The pattern

Every use case (`src/api/Teams.Core/UseCases/<Area>/<Verb>/`) is a pair of files:

- `<Verb>Command.cs` / `<Verb>Query.cs` — a plain record implementing `IRequest` (no response) or
  `IRequest<TResponse>`.
- `<Verb>CommandHandler.cs` / `<Verb>QueryHandler.cs` — implements
  `IRequestHandler<TRequest>`/`IRequestHandler<TRequest, TResponse>`, with one `HandleAsync`
  method and DI-injected dependencies.

Controllers depend only on `IMediator` and never touch a handler directly:

```csharp
public class GamesController(IMediator mediator) : ApiControllerBase
{
    public async Task<IActionResult> GetGames([FromQuery] GetGamesRequestModel query, CancellationToken ct)
    {
        var entities = await mediator.SendAsync(query.ToQuery(), ct);
        return Ok(entities.ToPagedList(GamesMapper.ToModel));
    }
}
```

## The trick: routing to a handler nobody named at compile time

`SendAsync(IRequest<TResponse> request, ...)` only knows the request as `IRequest<TResponse>` — it
has no `TRequest` type parameter to ask DI for `IRequestHandler<TRequest, TResponse>` directly.
`Mediator.SendAsync` (`CQRS/Concrete/Mediator.cs`) closes that gap with one reflection call:

```mermaid
sequenceDiagram
    participant Controller
    participant Mediator
    participant Wrapper as RequestHandlerWrapper&lt;TRequest,TResponse&gt;
    participant DI as IServiceProvider
    participant Handler as IRequestHandler&lt;TRequest,TResponse&gt;

    Controller->>Mediator: SendAsync(request)
    Mediator->>Mediator: requestType = request.GetType()
    Mediator->>Mediator: MakeGenericType(RequestHandlerWrapper&lt;,&gt;, requestType, TResponse)
    Mediator->>Wrapper: Activator.CreateInstance(wrapperType)
    Mediator->>Wrapper: HandleAsync(request, provider, ct)
    Wrapper->>DI: GetService(IRequestHandler&lt;TRequest,TResponse&gt;)
    DI-->>Wrapper: handler
    Wrapper->>Handler: HandleAsync((TRequest)request, ct)
    Handler-->>Wrapper: TResponse
    Wrapper-->>Mediator: TResponse
    Mediator-->>Controller: TResponse
```

`requestType` is only known at runtime, so `Mediator` builds the *closed* generic type
`RequestHandlerWrapper<MyCommand, MyResult>` via `MakeGenericType` and instantiates it through
`Activator.CreateInstance` — that instance gets boxed back down to the non-generic
`RequestHandlerWrapperBase<TResponse>`, which is the one abstraction `Mediator` can actually call
without knowing `TRequest`. The wrapper is the only place that ever gets to see the concrete
`TRequest` again, which is what lets it ask DI for the exact `IRequestHandler<TRequest, TResponse>`
and cast the request down to it.

Handler *registration* (`CQRS/Startup.cs`, run once at startup via `AddMediatorServices()`) is the
mirror image: scan the assembly for every concrete type implementing `IRequestHandler<>` or
`IRequestHandler<,>`, and register each one against its own closed interface as `Transient`. No
attributes, no manual registration list — add a `Command` + `Handler` pair and it's wired up.

## Why this is worth knowing

- **Zero third-party dependency** for a pattern that's normally reached for via MediatR.
- **The void/response split is real, not simulated** — `IRequest` and `IRequest<TResponse>` get
  genuinely different wrapper types (`RequestHandlerWrapper<TRequest>` vs.
  `RequestHandlerWrapper<TRequest, TResponse>`), with `Response.Void` existing purely as an
  internal placeholder so the non-generic wrapper still has *something* to return.
- **Every controller action is one line**: `mediator.SendAsync(command, ct)`. Business logic lives
  entirely in `Teams.Core/UseCases`, independently unit-testable with no ASP.NET Core in the
  picture at all.
