using Teams.Core.CQRS.Enums;
using Teams.Core.Exceptions;

namespace Teams.Core.CQRS.Wrappers;

/// <summary>
/// Describes a generic handler wrapper for an instance of <typeparamref name="TRequest"/> without return.
/// </summary>
/// <typeparam name="TRequest">The type of request handled.</typeparam>
internal class RequestHandlerWrapper<TRequest> : RequestHandlerWrapperBase
    where TRequest : IRequest
{
    /// <summary>
    /// Handler for an instance of <typeparamref name="TRequest"/> for which the handler does not return a value.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="provider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public override async Task<Response> HandleAsync(
        IRequest request,
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var handler = provider.GetService(typeof(IRequestHandler<TRequest>))
                          as IRequestHandler<TRequest>
            ?? throw RequestHandlerRegistrationException.ForRequest(typeof(TRequest));

        await handler.HandleAsync((TRequest)request, cancellationToken).ConfigureAwait(false);

        return Response.Void;
    }
}

/// <summary>Describes a generic handler wrapper for an instance of <typeparamref name="TRequest"/>.</summary>
/// <typeparam name="TRequest">The type of request handled.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
internal class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapperBase<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handler for am instance of <typeparamref name="TRequest"/> for which the handler returns an instance of
    /// <typeparamref name="TResponse"/>.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="provider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public override async Task<TResponse> HandleAsync(
        IRequest<TResponse> request,
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var handler = provider.GetService(typeof(IRequestHandler<TRequest, TResponse>))
                          as IRequestHandler<TRequest, TResponse>
                      ?? throw RequestHandlerRegistrationException.ForRequest(typeof(TRequest));

        return await handler.HandleAsync((TRequest)request, cancellationToken).ConfigureAwait(false);
    }
}