using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteTenantCommandHandler> _logger;

    public DeleteTenantCommandHandler(IApplicationDbContext context, ILogger<DeleteTenantCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        
        if (tenant == null)
            return Result.Failure("TenantNotFound", "Tenant not found.");

        tenant.Suspend(request.Reason);

        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant suspended with Id: {TenantId}, Reason: {Reason}", tenant.Id, request.Reason);

        return Result.Success();
    }
}
