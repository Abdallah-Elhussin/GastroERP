using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCompanyCommandHandler> _logger;

    public CreateCompanyCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateCompanyCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.TaxNumber,
            request.Dto.NameEn
        );

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company created: {CompanyId}", company.Id);

        return Result<CompanyDto>.Success(_mapper.Map<CompanyDto>(company));
    }
}

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateCompanyCommandHandler> _logger;

    public UpdateCompanyCommandHandler(IApplicationDbContext context, ILogger<UpdateCompanyCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (company == null) return Result.Failure("CompanyNotFound", "Company not found.");

        // Update contact info — Email/Phone are Value Objects, pass null for now (can be extended)
        company.UpdateContactInfo(null, null, request.Dto.Website);

        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company updated: {CompanyId}", company.Id);

        return Result.Success();
    }
}

public class ActivateCompanyCommandHandler : IRequestHandler<ActivateCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ActivateCompanyCommandHandler> _logger;

    public ActivateCompanyCommandHandler(IApplicationDbContext context, ILogger<ActivateCompanyCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (company == null) return Result.Failure("CompanyNotFound", "Company not found.");

        company.Activate();
        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company activated: {CompanyId}", company.Id);

        return Result.Success();
    }
}

public class DeactivateCompanyCommandHandler : IRequestHandler<DeactivateCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateCompanyCommandHandler> _logger;

    public DeactivateCompanyCommandHandler(IApplicationDbContext context, ILogger<DeactivateCompanyCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (company == null) return Result.Failure("CompanyNotFound", "Company not found.");

        company.Deactivate();
        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company deactivated: {CompanyId}", company.Id);

        return Result.Success();
    }
}
