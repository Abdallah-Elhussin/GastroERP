using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainProfile = GastroErp.Domain.Entities.Finance.TaxRegistrationProfile;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class TaxRegistrationMapper
{
    public static TaxRegistrationProfileDto ToDto(
        DomainProfile profile, string? companyName, string? branchName)
    {
        var certs = profile.Certificates
            .OrderByDescending(c => c.Version)
            .Select(c => new TaxRegistrationCertificateDto(
                c.Id, c.Version, c.FileName, c.StoragePath, c.ContentType,
                c.DocumentNumber, c.IssueDate, c.ExpiryDate, c.Notes, c.IsCurrent, c.UploadedAt))
            .ToList();

        return new TaxRegistrationProfileDto(
            profile.Id, profile.Number, profile.CompanyId, companyName,
            profile.BranchId, branchName, profile.VatNumber, profile.BranchVatNumber,
            profile.TaxOffice, profile.TaxpayerType, profile.ActivityCode,
            profile.ActivityNameAr, profile.ActivityNameEn, profile.DefaultTaxRate,
            profile.RegistrationDate, profile.ExpiryDate, profile.Status, profile.Notes,
            profile.IsSystem, profile.SortOrder, profile.HasBeenUsed,
            profile.CreatedAt, profile.CreatedBy, profile.UpdatedAt, profile.UpdatedBy,
            certs.FirstOrDefault(c => c.IsCurrent), certs);
    }
}

internal static class TaxRegistrationGuard
{
    public static async Task<Result?> ValidateUniquenessAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid companyId,
        Guid? branchId,
        string vatNumber,
        TaxRegistrationStatus status,
        Guid? excludeId,
        CancellationToken ct)
    {
        var vat = vatNumber.Trim();
        var vatTaken = await context.TaxRegistrationProfiles.AnyAsync(
            p => p.TenantId == tenantId && p.VatNumber == vat
                 && (excludeId == null || p.Id != excludeId), ct);
        if (vatTaken)
            return Result.Failure(ErrorCodes.TaxRegistrationVatDuplicate, "VAT number already exists.");

        if (status == TaxRegistrationStatus.Active)
        {
            var activeSameScope = await context.TaxRegistrationProfiles.AnyAsync(
                p => p.TenantId == tenantId
                     && p.CompanyId == companyId
                     && p.BranchId == branchId
                     && p.Status == TaxRegistrationStatus.Active
                     && (excludeId == null || p.Id != excludeId), ct);
            if (activeSameScope)
                return Result.Failure(ErrorCodes.TaxRegistrationActiveBranchDuplicate,
                    "An active tax registration already exists for this company/branch.");
        }

        return null;
    }

    public static bool IsAllowedCertificate(string fileName, string? contentType)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (ext is not (".pdf" or ".jpg" or ".jpeg" or ".png"))
            return false;
        if (string.IsNullOrWhiteSpace(contentType)) return true;
        var ct = contentType.ToLowerInvariant();
        return ct.Contains("pdf") || ct.Contains("jpeg") || ct.Contains("jpg") || ct.Contains("png");
    }
}

public sealed class CreateTaxRegistrationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateTaxRegistrationCommand, Result<TaxRegistrationProfileDto>>
{
    public async Task<Result<TaxRegistrationProfileDto>> Handle(
        CreateTaxRegistrationCommand request, CancellationToken cancellationToken)
    {
        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == request.Dto.CompanyId && c.TenantId == request.TenantId, cancellationToken);
        if (!companyOk)
            return Result<TaxRegistrationProfileDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        if (request.Dto.BranchId is Guid branchId)
        {
            var branchOk = await context.Branches.AnyAsync(
                b => b.Id == branchId && b.TenantId == request.TenantId && b.CompanyId == request.Dto.CompanyId,
                cancellationToken);
            if (!branchOk)
                return Result<TaxRegistrationProfileDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");
        }

        var uniqueness = await TaxRegistrationGuard.ValidateUniquenessAsync(
            context, request.TenantId, request.Dto.CompanyId, request.Dto.BranchId,
            request.Dto.VatNumber, request.Dto.Status, null, cancellationToken);
        if (uniqueness is not null)
            return Result<TaxRegistrationProfileDto>.Failure(
                uniqueness.ErrorCode!, uniqueness.ErrorMessage ?? "Validation failed.");

        try
        {
            var next = await context.TaxRegistrationProfiles.Where(p => p.TenantId == request.TenantId)
                .Select(p => (int?)p.Number).MaxAsync(cancellationToken) ?? 0;
            var number = next + 1;

            var profile = DomainProfile.Create(
                request.TenantId, number, request.Dto.CompanyId, request.Dto.VatNumber,
                request.Dto.BranchId, request.Dto.BranchVatNumber, request.Dto.TaxOffice,
                request.Dto.TaxpayerType, request.Dto.ActivityCode, request.Dto.ActivityNameAr,
                request.Dto.ActivityNameEn, request.Dto.DefaultTaxRate, request.Dto.RegistrationDate,
                request.Dto.ExpiryDate, request.Dto.Notes,
                request.Dto.SortOrder == 0 ? number : request.Dto.SortOrder);

            profile.SetStatus(request.Dto.Status);
            context.TaxRegistrationProfiles.Add(profile);
            await context.SaveChangesAsync(cancellationToken);
            return Result<TaxRegistrationProfileDto>.Success(
                await LoadDtoAsync(context, profile.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<TaxRegistrationProfileDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<TaxRegistrationProfileDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var profile = await context.TaxRegistrationProfiles.AsNoTracking()
            .Include(p => p.Certificates)
            .FirstAsync(p => p.Id == id, ct);
        return await EnrichAsync(context, profile, ct);
    }

    internal static async Task<TaxRegistrationProfileDto> EnrichAsync(
        IApplicationDbContext context, DomainProfile profile, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == profile.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        string? branch = null;
        if (profile.BranchId is Guid bid)
            branch = await context.Branches.AsNoTracking()
                .Where(b => b.Id == bid).Select(b => b.NameAr).FirstOrDefaultAsync(ct);
        return TaxRegistrationMapper.ToDto(profile, company, branch);
    }
}

public sealed class UpdateTaxRegistrationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTaxRegistrationCommand, Result<TaxRegistrationProfileDto>>
{
    public async Task<Result<TaxRegistrationProfileDto>> Handle(
        UpdateTaxRegistrationCommand request, CancellationToken cancellationToken)
    {
        var profile = await context.TaxRegistrationProfiles
            .Include(p => p.Certificates)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (profile is null)
            return Result<TaxRegistrationProfileDto>.Failure(
                ErrorCodes.TaxRegistrationNotFound, "Tax registration not found.");

        if (request.Dto.BranchId is Guid branchId)
        {
            var branchOk = await context.Branches.AnyAsync(
                b => b.Id == branchId && b.TenantId == profile.TenantId && b.CompanyId == request.Dto.CompanyId,
                cancellationToken);
            if (!branchOk)
                return Result<TaxRegistrationProfileDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");
        }

        var uniqueness = await TaxRegistrationGuard.ValidateUniquenessAsync(
            context, profile.TenantId, request.Dto.CompanyId, request.Dto.BranchId,
            request.Dto.VatNumber, request.Dto.Status, profile.Id, cancellationToken);
        if (uniqueness is not null)
            return Result<TaxRegistrationProfileDto>.Failure(
                uniqueness.ErrorCode!, uniqueness.ErrorMessage ?? "Validation failed.");

        try
        {
            profile.Update(
                request.Dto.CompanyId, request.Dto.BranchId, request.Dto.VatNumber,
                request.Dto.BranchVatNumber, request.Dto.TaxOffice, request.Dto.TaxpayerType,
                request.Dto.ActivityCode, request.Dto.ActivityNameAr, request.Dto.ActivityNameEn,
                request.Dto.DefaultTaxRate, request.Dto.RegistrationDate, request.Dto.ExpiryDate,
                request.Dto.Notes, request.Dto.SortOrder);
            profile.SetStatus(request.Dto.Status);
            context.TaxRegistrationProfiles.Update(profile);
            await context.SaveChangesAsync(cancellationToken);
            return Result<TaxRegistrationProfileDto>.Success(
                await CreateTaxRegistrationCommandHandler.LoadDtoAsync(context, profile.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<TaxRegistrationProfileDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteTaxRegistrationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteTaxRegistrationCommand, Result>
{
    public async Task<Result> Handle(DeleteTaxRegistrationCommand request, CancellationToken cancellationToken)
    {
        var profile = await context.TaxRegistrationProfiles
            .Include(p => p.Certificates)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (profile is null)
            return Result.Failure(ErrorCodes.TaxRegistrationNotFound, "Tax registration not found.");

        try { profile.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        profile.SoftDelete(null);
        context.TaxRegistrationProfiles.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class UploadTaxRegistrationCertificateCommandHandler(
    IApplicationDbContext context,
    IFileStorage fileStorage)
    : IRequestHandler<UploadTaxRegistrationCertificateCommand, Result<TaxRegistrationProfileDto>>
{
    public async Task<Result<TaxRegistrationProfileDto>> Handle(
        UploadTaxRegistrationCertificateCommand request, CancellationToken cancellationToken)
    {
        var profile = await context.TaxRegistrationProfiles
            .Include(p => p.Certificates)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (profile is null)
            return Result<TaxRegistrationProfileDto>.Failure(
                ErrorCodes.TaxRegistrationNotFound, "Tax registration not found.");

        if (!TaxRegistrationGuard.IsAllowedCertificate(request.FileName, request.ContentType))
            return Result<TaxRegistrationProfileDto>.Failure(
                ErrorCodes.TaxRegistrationCertificateInvalid,
                "Certificate must be PDF, JPG, or PNG.");

        try
        {
            var path = await fileStorage.UploadAsync(
                request.FileName, request.Content, request.ContentType ?? "application/octet-stream",
                cancellationToken);

            profile.AddCertificate(
                request.FileName, path, request.ContentType,
                request.DocumentNumber, request.IssueDate, request.ExpiryDate, request.Notes);

            // Prefer document meta from upsert when provided via same request fields.
            context.TaxRegistrationProfiles.Update(profile);
            await context.SaveChangesAsync(cancellationToken);
            return Result<TaxRegistrationProfileDto>.Success(
                await CreateTaxRegistrationCommandHandler.LoadDtoAsync(context, profile.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<TaxRegistrationProfileDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
