using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    private const string Url = "api/v1/users";
    private const string VersionlessUrl = "api/users";

    public abstract class UsersControllerTestsBase(ApiWebApplicationFactory factory)
        : ApiControllerTestsBase(factory), IAsyncLifetime
    {
        protected IReadOnlyList<User> SeedUsers { get; } = Enumerable.Range(1, 30).Select(CreateSeedUser).ToList();

        protected static readonly DateTimeOffset BaseDate = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        public virtual async ValueTask InitializeAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            await context.Users.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Users.AddRangeAsync(SeedUsers, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private static User CreateSeedUser(int index) =>
            EntityFactory.CreateUser(
                id: $"user-{index:D3}",
                displayName: $"Test User {index:D3}",
                externalId: $"external-{index:D3}",
                email: $"user{index:D3}@test.net",
                dateCreated: BaseDate.AddDays(index),
                postCreationSteps: user => user.ApplyRatingChange(index)); // Ratings: 1001 - 1030, cycling deterministically with the index.
    }
}