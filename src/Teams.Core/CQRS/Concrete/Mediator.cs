using Teams.Core.CQRS.Wrappers;
using Teams.Core.Exceptions;

namespace Teams.Core.CQRS.Concrete;

/// <summary>Service responsible for sending CQRS requests.</summary>
/// <param name="serviceProvider">The service provider.</param>
internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    /// <inheritdoc />
    public Task SendAsync(IRequest request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        // We don't know what type of IRequest is provided, so we need to work out the appropriate wrapper type (i.e.
        // RequestHandlerWrapper<MyRequest> where MyRequest : IRequest).
        var wrapperType = typeof(RequestHandlerWrapper<>).MakeGenericType(requestType);

        // Since we can't pass wrapperType as a generic type parameter, we have to create an instance of wrapperType
        // but box as its base type, RequestHandlerWrapperBase.
        var handler = Activator.CreateInstance(wrapperType) as RequestHandlerWrapperBase
                      ?? throw RequestHandlerException.ForFailedInstantiation(requestType);

        // Then we can call the HandleAsync(IRequest, IServiceProvider, CancellationToken) method that is abstract in
        // the base type, but defined in the derived implementation type.
        return handler.HandleAsync(request, serviceProvider, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = Activator.CreateInstance(wrapperType) as RequestHandlerWrapperBase<TResponse>
                      ?? throw RequestHandlerException.ForFailedInstantiation(requestType);
        return handler.HandleAsync(request, serviceProvider, cancellationToken);
    }
}