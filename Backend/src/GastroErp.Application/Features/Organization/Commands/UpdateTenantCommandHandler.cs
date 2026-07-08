using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(IApplicationDbContext context, ILogger<UpdateTenantCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        
        if (tenant == null)
            return Result.Failure("TenantNotFound", "Tenant not found.");

        tenant.UpdateDefaults(
            request.Dto.DefaultCurrency,
            request.Dto.DefaultLanguage,
            request.Dto.DefaultTimezone
        );

        tenant.UpdateBranding(
            request.Dto.LogoUrl,
            request.Dto.PrimaryColor,
            request.Dto.SecondaryColor
        );

        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant updated with Id: {TenantId}", tenant.Id);

        return Result.Success();
    }
}
