using FluentValidation;
using Teams.Api.Controllers.V1.Players.RequestModels;

namespace Teams.Api.Controllers.V1.Players.Validators;

public class CreatePlayerValidator : AbstractValidator<CreatePlayerRequestModel>
{
    public CreatePlayerValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.UserId)
            .MaximumLength(100)
            .When(request => request.UserId is not null);
    }
}