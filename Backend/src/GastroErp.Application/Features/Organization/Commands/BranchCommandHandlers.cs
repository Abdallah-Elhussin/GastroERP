using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

internal static class BranchCodingMapper
{
    public static BranchDto ToDto(Branch branch, string? companyName)
    {
        return new BranchDto(
            branch.Id,
            branch.TenantId,
            branch.CompanyId,
            branch.NameAr,
            branch.NameEn,
            branch.Code,
            branch.BranchType,
            branch.Status,
            branch.PhoneNumber,
            branch.Email,
            branch.Address.StreetAr,
            branch.Address.StreetEn,
            branch.Address.CityAr,
            branch.Address.CityEn,
            branch.AllowNegativeStock,
            branch.AllowOfflineSales,
            branch.CreatedAt.UtcDateTime,
            branch.UpdatedAt,
            companyName,
            branch.IsActive);
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateBranchCommandHandler> _logger;

    public CreateBranchCommandHandler(IApplicationDbContext context, ILogger<CreateBranchCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var tenantId = dto.TenantId is Guid t && t != Guid.Empty
            ? t
            : await _context.Companies.AsNoTracking()
                .Where(c => c.Id == dto.CompanyId)
                .Select(c => c.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

        if (tenantId == Guid.Empty)
            return Result<BranchDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        var companyOk = await _context.Companies.AnyAsync(
            c => c.Id == dto.CompanyId && c.TenantId == tenantId, cancellationToken);
        if (!companyOk)
            return Result<BranchDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        var nameAr = dto.NameAr.Trim();
        if (string.IsNullOrWhiteSpace(nameAr))
            return Result<BranchDto>.Failure(ErrorCodes.NameArRequired, "Branch name is required.");

        if (await _context.Branches.AnyAsync(
                b => b.TenantId == tenantId && b.CompanyId == dto.CompanyId && b.NameAr == nameAr,
                cancellationToken))
            return Result<BranchDto>.Failure(ErrorCodes.BranchNameDuplicate,
                "A branch with the same name already exists for this company.");

        var code = dto.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            var maxCode = await _context.Branches.AsNoTracking()
                .Where(b => b.TenantId == tenantId && b.Code != null && b.Code.Length <= 10)
                .Select(b => b.Code)
                .ToListAsync(cancellationToken);
            var next = maxCode
                .Select(c => int.TryParse(c, out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;
            code = next.ToString();
        }

        var branch = new Branch(tenantId, dto.CompanyId, nameAr, dto.BranchType, dto.NameEn, code);
        if (!string.IsNullOrWhiteSpace(dto.Location))
            branch.UpdateAddress(new Address(null, null, dto.Location.Trim(), null, null, null, null));

        if (!dto.IsActive)
            branch.Deactivate();

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch created with Id: {BranchId}", branch.Id);

        var companyName = await _context.Companies.AsNoTracking()
            .Where(c => c.Id == branch.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(cancellationToken);
        return Result<BranchDto>.Success(BranchCodingMapper.ToDto(branch, companyName));
    }
}

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateBranchCommandHandler> _logger;

    public UpdateBranchCommandHandler(IApplicationDbContext context, ILogger<UpdateBranchCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch is null)
            return Result<BranchDto>.Failure(ErrorCodes.OrgBranchNotFound, "Branch not found.");

        var dto = request.Dto;
        var nameAr = dto.NameAr.Trim();
        if (string.IsNullOrWhiteSpace(nameAr))
            return Result<BranchDto>.Failure(ErrorCodes.NameArRequired, "Branch name is required.");

        if (await _context.Branches.AnyAsync(
                b => b.TenantId == branch.TenantId && b.CompanyId == branch.CompanyId
                     && b.NameAr == nameAr && b.Id != branch.Id, cancellationToken))
            return Result<BranchDto>.Failure(ErrorCodes.BranchNameDuplicate,
                "A branch with the same name already exists for this company.");

        // CompanyId is immutable after create (form shows company read-only on edit).
        branch.UpdateInfo(nameAr, dto.NameEn, dto.Code);
        branch.UpdateContactInfo(dto.Email, dto.PhoneNumber);
        branch.ConfigureSettings(dto.AllowNegativeStock, dto.AllowOfflineSales);

        var location = dto.Location?.Trim();
        branch.UpdateAddress(new Address(
            branch.Address.StreetAr, branch.Address.StreetEn,
            location, branch.Address.CityEn,
            branch.Address.RegionAr, branch.Address.RegionEn,
            branch.Address.PostalCode));

        if (dto.IsActive) branch.Activate();
        else branch.Deactivate();

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch updated: {BranchId}", branch.Id);

        var companyName = await _context.Companies.AsNoTracking()
            .Where(c => c.Id == branch.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(cancellationToken);
        return Result<BranchDto>.Success(BranchCodingMapper.ToDto(branch, companyName));
    }
}

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteBranchCommandHandler> _logger;

    public DeleteBranchCommandHandler(IApplicationDbContext context, ILogger<DeleteBranchCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch is null)
            return Result.Failure(ErrorCodes.OrgBranchNotFound, "Branch not found.");

        if (branch.IsDefault)
            return Result.Failure(ErrorCodes.RequiredField, "Cannot delete the default branch.");

        branch.SoftDelete(null);
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch deleted: {BranchId}", branch.Id);
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
        if (branch is null) return Result.Failure(ErrorCodes.OrgBranchNotFound, "Branch not found.");

        if (request.IsActive) branch.Activate();
        else branch.Deactivate();

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
        if (branch is null) return Result.Failure(ErrorCodes.OrgBranchNotFound, "Branch not found.");

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
        if (branch is null) return Result.Failure(ErrorCodes.OrgBranchNotFound, "Branch not found.");

        branch.RestoreFromArchive();
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Branch restored: {BranchId}", branch.Id);
        return Result.Success();
    }
}
