using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Invitations.RequestModels;

namespace Teams.Api.Controllers.V1.Invitations.Examples;

[ExcludeFromCodeCoverage]
public class CreateInvitationsRequestModelExample : IExamplesProvider<CreateInvitationsRequestModel>
{
    public CreateInvitationsRequestModel GetExamples() => CreateInvitationsRequestModel.Example;
}