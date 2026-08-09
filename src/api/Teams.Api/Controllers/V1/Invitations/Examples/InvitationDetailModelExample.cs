using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;

namespace Teams.Api.Controllers.V1.Invitations.Examples;

[ExcludeFromCodeCoverage]
public class InvitationDetailModelExample : IExamplesProvider<InvitationDetailModel>
{
    public InvitationDetailModel GetExamples() => InvitationDetailModel.Example;
}