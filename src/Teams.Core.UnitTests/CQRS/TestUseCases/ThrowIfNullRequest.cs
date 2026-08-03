using Teams.Core.CQRS;

namespace Teams.Core.UnitTests.CQRS.TestUseCases;

public record ThrowIfNullRequest(string? Value) : IRequest;