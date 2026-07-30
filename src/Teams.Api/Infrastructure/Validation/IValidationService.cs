namespace Teams.Api.Infrastructure.Validation;

public interface IValidationService
{
    /// <summary>
    /// Validates the given query.  Throws QueryValidationException when validation fails.
    /// </summary>
    Task ValidateQueryAsync<T>(T query, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the given command.  Throws CommandValidationException when validation fails.
    /// </summary>
    Task ValidateCommandAsync<T>(T command, CancellationToken cancellationToken);
}