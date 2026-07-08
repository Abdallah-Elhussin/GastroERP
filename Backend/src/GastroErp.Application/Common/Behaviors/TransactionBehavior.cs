using System;
using System.Threading;
using System.Threading.Tasks;
using GastroErp.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IApplicationDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Implement proper transaction handling in infrastructure layer (ExecutionStrategy)
        // For now just passing through, this will be handled properly with IExecutionStrategy later.
        return await next();
    }
}
