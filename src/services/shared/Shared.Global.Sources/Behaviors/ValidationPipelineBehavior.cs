using MediatR;
using FluentValidation;
using CQRS.MediatR.Helper.ErrorHandler;

namespace Shared.Global.Sources.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest,TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators, nameof(validators));

        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        ValidationError[] errors = _validators
            .Select(async validator 
                => await validator.ValidateAsync(request))
            .SelectMany(validationResult 
                => validationResult.Result.Errors)
            .Where(validationFailure 
                => validationFailure is not null)
            .Select(failure 
                => new ValidationError(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Any())
            throw new CQRS.MediatR.Helper.Exceptions.ValidationException(errors);

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(ValidationError[] errors)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
            return (ValidationResult.WithErrors(errors) as TResult)!;

        object validationResult = typeof(ValidationResult<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(ValidationResult.WithErrors))!
            .Invoke(null, new object?[] { errors })!;

        return (TResult)validationResult;
    }
}

