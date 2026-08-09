using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Common.Pagination;

namespace Teams.Api.Controllers.V1.Invitations.Examples;

[ExcludeFromCodeCoverage]
public class InvitationModelPageExample : IExamplesProvider<PagedList<InvitationModel>>
{
    public PagedList<InvitationModel> GetExamples() => new(
        Data: [InvitationModel.Example],
        Cursor: "MTc4NTUyNDI1ODQ0NQ==",
        Count: 1);
}