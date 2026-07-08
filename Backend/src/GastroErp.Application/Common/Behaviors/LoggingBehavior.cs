using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GastroErp.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUser _currentUser;

    public LoggingBehavior(ILogger<TRequest> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.Id;
        var userName = _currentUser.Name;

        _logger.LogInformation("GastroErp Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);

        var response = await next();

        return response;
    }
}
