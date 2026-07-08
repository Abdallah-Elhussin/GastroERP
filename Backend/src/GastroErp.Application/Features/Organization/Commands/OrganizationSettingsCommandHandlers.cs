using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Features.Organization.Commands;

public class UpdateOrganizationSettingsCommandHandler : IRequestHandler<UpdateOrganizationSettingsCommand, Result<OrganizationSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateOrganizationSettingsCommandHandler> _logger;

    public UpdateOrganizationSettingsCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<UpdateOrganizationSettingsCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrganizationSettingsDto>> Handle(UpdateOrganizationSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _context.OrganizationSettings
            .FirstOrDefaultAsync(s => s.TenantId == request.TenantId, cancellationToken);

        if (settings == null)
        {
            settings = new OrganizationSettings(
                request.TenantId, 
                request.Dto.CompanyName, 
                request.Dto.LegalName, 
                request.Dto.CommercialRegistration, 
                request.Dto.TaxNumber);
            
            settings.UpdateLocalization(
                request.Dto.DefaultCurrencyId,
                request.Dto.DefaultLanguageId,
                request.Dto.DefaultTimezoneId,
                request.Dto.DateFormat,
                request.Dto.NumberFormat);
            
            settings.UpdateAppearance(request.Dto.LogoUrl, request.Dto.Theme);
            settings.UpdateContactInfo(request.Dto.Address, request.Dto.ContactEmail, request.Dto.ContactPhone);

            _context.OrganizationSettings.Add(settings);
        }
        else
        {
            settings.UpdateGeneralInfo(
                request.Dto.CompanyName, 
                request.Dto.LegalName, 
                request.Dto.CommercialRegistration, 
                request.Dto.TaxNumber);
            
            settings.UpdateLocalization(
                request.Dto.DefaultCurrencyId,
                request.Dto.DefaultLanguageId,
                request.Dto.DefaultTimezoneId,
                request.Dto.DateFormat,
                request.Dto.NumberFormat);
            
            settings.UpdateAppearance(request.Dto.LogoUrl, request.Dto.Theme);
            settings.UpdateContactInfo(request.Dto.Address, request.Dto.ContactEmail, request.Dto.ContactPhone);

            _context.OrganizationSettings.Update(settings);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("OrganizationSettings updated for TenantId: {TenantId}", request.TenantId);

        return Result<OrganizationSettingsDto>.Success(_mapper.Map<OrganizationSettingsDto>(settings));
    }
}
