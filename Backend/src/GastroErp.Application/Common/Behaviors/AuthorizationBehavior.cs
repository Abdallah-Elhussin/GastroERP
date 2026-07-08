using System.Threading;
using System.Threading.Tasks;
using GastroErp.Application.Common.Interfaces;
using MediatR;

namespace GastroErp.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUser _currentUser;

    public AuthorizationBehavior(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Add authorization logic based on attributes or interfaces on TRequest
        // For example, checking [Authorize] attribute equivalent
        
        return await next();
    }
}
