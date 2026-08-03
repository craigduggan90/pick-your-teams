using FluentValidation;
using FluentValidation.Results;
using Teams.Data.Repositories.Games;
using Teams.Data.Repositories.Players;
using Teams.Data.Repositories.Users;
using Teams.Data.Services;

namespace Teams.Core.UnitTests.UseCases;

public abstract class UseCaseTestBase<TRequest>
{
    protected IUsersRepository UsersRepository { get; } = Substitute.For<IUsersRepository>();

    protected IGamesRepository GamesRepository { get; } = Substitute.For<IGamesRepository>();

    protected IPlayersRepository PlayersRepository { get; } = Substitute.For<IPlayersRepository>();

    protected IUnitOfWork UnitOfWork { get; }

    protected IValidator<TRequest> Validator { get; } = Substitute.For<IValidator<TRequest>>();

    protected UseCaseTestBase()
    {
        UnitOfWork = Substitute.For<IUnitOfWork>();
        UnitOfWork.Users.Returns(UsersRepository);
        UnitOfWork.Games.Returns(GamesRepository);
        UnitOfWork.Players.Returns(PlayersRepository);

        // Passes validation by default - call SetupValidator again in a test to override this.
        SetupValidator();
    }

    /// <summary>Configures <see cref="Validator"/> to return <paramref name="result"/>.</summary>
    protected void SetupValidator(ValidationResult? result = null) =>
        Validator.ValidateAsync(Arg.Any<TRequest>(), Arg.Any<CancellationToken>()).Returns(result ?? new ValidationResult());

    /// <summary>A single-error <see cref="ValidationResult"/>, for exercising the validation-failure path.</summary>
    protected static ValidationResult InvalidResult(string propertyName = "PropertyName", string errorMessage = "Error message") =>
        new([new ValidationFailure(propertyName, errorMessage)]);
}