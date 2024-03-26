namespace Ownat.Common;

using ErrorOr;
using FluentValidation;
using MediatR;

/// <summary>
/// This class is a pipeline behavior that validates the request using FluentValidation.
/// </summary>
/// <param name="validator">Booking request validators.</param>
/// <typeparam name="TRequest">Generic request.</typeparam>
/// <typeparam name="TResponse">Generic response.</typeparam>
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await next();
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .ConvertAll(error => Error.Validation(
                code: error.PropertyName,
                description: error.ErrorMessage));

        return (dynamic)errors;
    }
}