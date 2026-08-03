using FluentValidation;

namespace Teams.Core.UseCases.Users.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(user => user.DisplayName)
            .MaximumLength(100);

        RuleFor(user => user.Tag)
            .MaximumLength(36);

        RuleFor(user => user.Email)
            .EmailAddress();

        RuleFor(user => user.Mobile)
            .MaximumLength(100);
    }
}