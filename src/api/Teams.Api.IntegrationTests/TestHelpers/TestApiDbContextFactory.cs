using Teams.Data.Context;

namespace Teams.Api.IntegrationTests.TestHelpers;

public class TestApiDbContextFactory(ApiDbContext context) : IApiDbContextFactory
{
    public ApiDbContext CreateDbContext() => context;

    public ApiDbContext CreateDbContext(ContextType contextType) => context;
}