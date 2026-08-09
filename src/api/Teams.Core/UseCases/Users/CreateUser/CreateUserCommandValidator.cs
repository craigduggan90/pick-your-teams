using FluentValidation;

namespace Teams.Core.UseCases.Users.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(user => user.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(user => user.ExternalId)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(user => user.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(user => user.Mobile)
            .MaximumLength(100);
    }
}