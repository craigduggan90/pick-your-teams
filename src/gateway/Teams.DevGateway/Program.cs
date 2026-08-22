using Teams.DevGateway.Authorisation;

// Local-only stand-in for AWS API Gateway (see claude.md's Auth model section). No auth logic of
// its own: every request is authorised by calling Teams.Authoriser.LocalHost, and the verdict is
// either a 401 or a forward through to the real, unmodified Teams.Api. Must never ship anywhere —
// it lives outside Teams.sln for exactly that reason, same guarantee Teams.Api.EndToEndTests has.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IAuthoriserClient, HttpAuthoriserClient>(client =>
{
    var authoriserBaseUrl = builder.Configuration["Authoriser:BaseUrl"]
        ?? throw new InvalidOperationException("Authoriser:BaseUrl is not configured.");
    client.BaseAddress = new Uri(authoriserBaseUrl);
});
builder.Services.AddScoped<AuthorisationHandler>();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.Use(async (context, next) =>
{
    var authorisationHandler = context.RequestServices.GetRequiredService<AuthorisationHandler>();
    var authorizationHeader = context.Request.Headers.Authorization.ToString();

    var decision = await authorisationHandler.DecideAsync(
        string.IsNullOrEmpty(authorizationHeader) ? null : authorizationHeader,
        context.Request.Path,
        context.Request.Method,
        context.RequestAborted);

    if (!decision.IsAllowed)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    await next();
});

app.MapReverseProxy();

app.Run();