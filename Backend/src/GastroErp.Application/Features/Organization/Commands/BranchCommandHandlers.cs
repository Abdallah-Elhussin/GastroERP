using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBranchCommandHandler> _logger;

    public CreateBranchCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateBranchCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = new Branch(
            request.Dto.TenantId,
            request.Dto.CompanyId,
            request.Dto.NameAr,
            request.Dto.BranchType,
            request.Dto.NameEn,
            request.Dto.Code
        );

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch created with Id: {BranchId}", branch.Id);

        return Result<BranchDto>.Success(_mapper.Map<BranchDto>(branch));
    }
}

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateBranchCommandHandler> _logger;

    public UpdateBranchCommandHandler(IApplicationDbContext context, ILogger<UpdateBranchCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null) return Result.Failure("BranchNotFound", "Branch not found.");

        branch.UpdateContactInfo(request.Dto.Email, request.Dto.PhoneNumber);
        branch.ConfigureSettings(request.Dto.AllowNegativeStock, request.Dto.AllowOfflineSales);

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch updated: {BranchId}", branch.Id);

        return Result.Success();
    }
}



public class ChangeBranchStatusCommandHandler : IRequestHandler<ChangeBranchStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ChangeBranchStatusCommandHandler> _logger;

    public ChangeBranchStatusCommandHandler(IApplicationDbContext context, ILogger<ChangeBranchStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeBranchStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null) return Result.Failure("BranchNotFound", "Branch not found.");

        if (request.IsActive)
        {
            branch.Activate();
        }
        else
        {
            branch.Deactivate();
        }

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch status changed to {IsActive}: {BranchId}", request.IsActive, branch.Id);

        return Result.Success();
    }
}

public class ArchiveBranchCommandHandler : IRequestHandler<ArchiveBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ArchiveBranchCommandHandler> _logger;

    public ArchiveBranchCommandHandler(IApplicationDbContext context, ILogger<ArchiveBranchCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ArchiveBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null) return Result.Failure("BranchNotFound", "Branch not found.");

        branch.Archive();

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch archived: {BranchId}", branch.Id);

        return Result.Success();
    }
}

public class RestoreBranchCommandHandler : IRequestHandler<RestoreBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RestoreBranchCommandHandler> _logger;

    public RestoreBranchCommandHandler(IApplicationDbContext context, ILogger<RestoreBranchCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(RestoreBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null) return Result.Failure("BranchNotFound", "Branch not found.");

        branch.RestoreFromArchive();

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch restored: {BranchId}", branch.Id);

        return Result.Success();
    }
}
