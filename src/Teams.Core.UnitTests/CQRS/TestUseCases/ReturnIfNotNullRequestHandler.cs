using Teams.Core.CQRS;

namespace Teams.Core.UnitTests.CQRS.TestUseCases;

public class ReturnIfNotNullRequestHandler : IRequestHandler<ReturnIfNotNullRequest, string>
{
    public Task<string> HandleAsync(ReturnIfNotNullRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(request.Value, nameof(request.Value));
        return Task.FromResult(request.Value);
    }
}