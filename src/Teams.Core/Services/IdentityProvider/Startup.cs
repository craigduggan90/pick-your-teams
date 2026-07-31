using Auth0.ManagementApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Teams.Core.Services.IdentityProvider;

public static class Startup
{
    public static IHostApplicationBuilder AddIdentityProviderServices(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<IdpSettings>(builder.Configuration.GetSection("IdP"));
        builder.Services.AddSingleton<IdpSettings>(provider => provider.GetRequiredService<IOptions<IdpSettings>>().Value);

        builder.Services.AddSingleton<IManagementApiClient>(provider =>
        {
            var settings = provider.GetRequiredService<IdpSettings>();
            return new ManagementClient(new ManagementClientOptions
            {
                Domain = settings.Domain,
                TokenProvider = new ClientCredentialsTokenProvider(
                    domain: settings.Domain,
                    clientId: settings.ClientId,
                    clientSecret: settings.ClientSecret
                )
            });
        });

        builder.Services.AddScoped<IIdpClient, IdpClient>();

        return builder;
    }
}