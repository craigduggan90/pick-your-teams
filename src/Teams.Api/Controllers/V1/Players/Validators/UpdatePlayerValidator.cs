using FluentValidation;
using Teams.Api.Controllers.V1.Players.RequestModels;

namespace Teams.Api.Controllers.V1.Players.Validators;

public class UpdatePlayerValidator : AbstractValidator<UpdatePlayerRequestModel>
{
    public UpdatePlayerValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}