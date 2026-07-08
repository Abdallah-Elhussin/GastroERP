using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Features.Organization.Queries;

public class GetOrganizationSettingsQueryHandler : IRequestHandler<GetOrganizationSettingsQuery, Result<OrganizationSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrganizationSettingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationSettingsDto>> Handle(GetOrganizationSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.OrganizationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == request.TenantId, cancellationToken);

        if (settings == null)
        {
            return Result<OrganizationSettingsDto>.Failure("NotFound", "Organization settings not found.");
        }

        return Result<OrganizationSettingsDto>.Success(_mapper.Map<OrganizationSettingsDto>(settings));
    }
}
