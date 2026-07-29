using FluentValidation;
using FluentValidation.Results;
using Teams.Core.Exceptions;

namespace Teams.Core.Services.Validation;

public class ValidationService : IValidationService
{
    private readonly Dictionary<Type, IValidator> _validatorsByType;

    public ValidationService(IEnumerable<IValidator> validators)
    {
        _validatorsByType = validators
            .SelectMany(validator => validator.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => (RequestType: i.GetGenericArguments()[0], Validator: validator)))
            .ToDictionary(x => x.RequestType, x => x.Validator);
    }

    public async Task ValidateQueryAsync<T>(T query, CancellationToken cancellationToken) =>
        QueryValidationException.ThrowIfValidationFailed(await ValidateAsync(query, cancellationToken));

    public async Task ValidateCommandAsync<T>(T command, CancellationToken cancellationToken) =>
        CommandValidationException.ThrowIfValidationFailed(await ValidateAsync(command, cancellationToken));

    private Task<ValidationResult> ValidateAsync<T>(T request, CancellationToken cancellationToken)
    {
        return _validatorsByType.TryGetValue(typeof(T), out var validator)
            ? validator.ValidateAsync(new ValidationContext<T>(request), cancellationToken)
            : throw new ValidatorResolverException($"No validator found for '{typeof(T).Name}'.");
    }
}