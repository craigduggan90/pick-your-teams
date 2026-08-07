using FluentValidation;
using Teams.Common;

namespace Teams.Core.UseCases.Users.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(user => user.DisplayName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .When(user => user.DisplayName is not null);

        RuleFor(user => user.Tag)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(36)
            .Matches(Constants.TagRegexPattern)
            .When(user => user.Tag is not null);

        RuleFor(user => user.Email)
            .EmailAddress();

        RuleFor(user => user.Mobile)
            .MaximumLength(100);
    }
}