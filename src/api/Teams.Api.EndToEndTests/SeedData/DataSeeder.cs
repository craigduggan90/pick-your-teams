using Microsoft.EntityFrameworkCore;
using Teams.Data.Services;

namespace Teams.Api.EndToEndTests.SeedData;

public class DataSeeder(IUnitOfWork uow) : IDataSeeder
{
    public async Task SetupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await uow.Context.Database.EnsureDeletedAsync(cancellationToken);
        await uow.Context.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    public async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        await uow.Context.Users.AddRangeAsync(SeedDataFactory.SeedUsers, cancellationToken: cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }
}