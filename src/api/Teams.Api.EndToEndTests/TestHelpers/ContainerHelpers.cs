namespace Teams.Api.EndToEndTests.TestHelpers;

public static class ContainerHelpers
{
    public static IServiceCollection RemoveService<TService>(this IServiceCollection services)
    {
        var service = services.SingleOrDefault(descriptor => descriptor.ServiceType == typeof(TService));
        if (service is null)
            return services;

        services.Remove(service);
        return services;
    }
}