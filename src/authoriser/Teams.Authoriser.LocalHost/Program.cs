using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Teams.Authoriser;

// Local-dev-only host for the Teams.Authoriser Lambda function: no SAM CLI, no LocalStack, no
// AWS CLI dependency. It just deserializes an HTTP POST into the same request type API Gateway
// would build for a REQUEST-type custom authorizer, invokes the real Function.FunctionHandler,
// and serializes the real response type back out. Teams.DevGateway is the only intended caller.
// (For interactive/manual testing of Function.cs with a debugger attached, use
// Amazon.Lambda.TestTool instead — see README.md.)
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var function = new Function();

app.MapPost("/authorize", async (APIGatewayCustomAuthorizerRequest request) =>
{
    var response = await function.FunctionHandler(request, new TestLambdaContext());
    return Results.Ok(response);
});

app.Run();