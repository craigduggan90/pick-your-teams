namespace Teams.Api.EndToEndTests.SeedData;

public interface IDataSeeder
{
    Task SetupDatabaseAsync(CancellationToken cancellationToken = default);
    
    Task SeedDataAsync(CancellationToken cancellationToken = default);
}