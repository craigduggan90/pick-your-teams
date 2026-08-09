using Teams.Core.CQRS;

namespace Teams.Core.UnitTests.CQRS.TestUseCases;

public record ReturnIfNotNullRequest(string? Value) : IRequest<string>;