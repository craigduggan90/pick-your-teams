using FluentValidation;
using FluentValidation.Results;
using Teams.Core.Models;
using Teams.Core.Services;
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

    protected IActorAccessor ActorAccessor { get; } = Substitute.For<IActorAccessor>();

    protected UseCaseTestBase()
    {
        UnitOfWork = Substitute.For<IUnitOfWork>();
        UnitOfWork.Users.Returns(UsersRepository);
        UnitOfWork.Games.Returns(GamesRepository);
        UnitOfWork.Players.Returns(PlayersRepository);

        UsersRepository.CreateAsync(Arg.Any<Domain.Entities.User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Domain.Entities.User>(0));

        UsersRepository.UpdateAsync(Arg.Any<Domain.Entities.User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Domain.Entities.User>(0));

        GamesRepository.CreateAsync(Arg.Any<Domain.Entities.Game>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Domain.Entities.Game>(0));

        GamesRepository.UpdateAsync(Arg.Any<Domain.Entities.Game>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Domain.Entities.Game>(0));

        PlayersRepository.CreateAsync(Arg.Any<Domain.Entities.Player>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Domain.Entities.Player>(0));

        PlayersRepository.UpdateAsync(Arg.Any<Domain.Entities.Player>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Domain.Entities.Player>(0));

        ActorAccessor.Current.Returns(new Actor("organiser-id", "organiser-tag", "organiser-display-name"));

        SetupValidator();
    }

    protected void SetupValidator(ValidationResult? result = null) =>
        Validator.ValidateAsync(Arg.Any<TRequest>(), Arg.Any<CancellationToken>()).Returns(result ?? new ValidationResult());

    protected static ValidationResult InvalidResult(string propertyName = "PropertyName", string errorMessage = "Error message") =>
        new([new ValidationFailure(propertyName, errorMessage)]);
}