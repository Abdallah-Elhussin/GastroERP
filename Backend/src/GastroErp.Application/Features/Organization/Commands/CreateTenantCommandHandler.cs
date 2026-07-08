using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantMasterDataSeedService _masterDataSeed;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        IApplicationDbContext context, IMapper mapper,
        ITenantMasterDataSeedService masterDataSeed, ILogger<CreateTenantCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _masterDataSeed = masterDataSeed;
        _logger = logger;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant(
            request.Dto.NameAr,
            request.Dto.Slug,
            request.Dto.DefaultCurrency,
            request.Dto.DefaultLanguage,
            request.Dto.DefaultTimezone,
            request.Dto.NameEn
        );

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        await _masterDataSeed.SeedAsync(tenant.Id, cancellationToken);

        _logger.LogInformation("Tenant created with Id: {TenantId}", tenant.Id);

        var tenantDto = _mapper.Map<TenantDto>(tenant);
        return Result<TenantDto>.Success(tenantDto);
    }
}
