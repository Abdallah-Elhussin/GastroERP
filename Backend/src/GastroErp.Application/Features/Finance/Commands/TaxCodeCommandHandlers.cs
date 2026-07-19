using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainTaxCode = GastroErp.Domain.Entities.Finance.TaxCode;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class TaxCodeMapper
{
    public static TaxCodeDto ToDto(
        DomainTaxCode entity,
        string? companyName,
        string? branchName,
        string? salesAccountNumber,
        string? salesAccountName,
        string? purchaseAccountNumber,
        string? purchaseAccountName)
    {
        var rates = entity.Rates
            .OrderBy(r => r.FromDate)
            .Select(r => new TaxCodeRateDto(r.Id, r.FromDate, r.ToDate, r.Rate))
            .ToList();

        return new TaxCodeDto(
            entity.Id,
            entity.Number,
            entity.CompanyId,
            companyName,
            entity.BranchId,
            branchName,
            entity.Code,
            entity.NameAr,
            entity.NameEn,
            entity.AppliesTo,
            entity.CalculationMethod,
            entity.SalesAccountId,
            salesAccountNumber,
            salesAccountName,
            entity.PurchaseAccountId,
            purchaseAccountNumber,
            purchaseAccountName,
            entity.PriceIncludesTax,
            entity.IsActive,
            entity.HasBeenUsed,
            entity.CurrentRate(),
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy,
            rates);
    }
}

internal static class TaxCodeGuard
{
    public static void ApplyRates(IApplicationDbContext context, DomainTaxCode entity, IReadOnlyList<TaxCodeRateDto>? rates)
    {
        var existing = entity.Rates.ToList();
        if (existing.Count > 0)
        {
            context.TaxCodeRates.RemoveRange(existing);
            foreach (var row in existing)
                entity.RemoveRate(row.Id);
        }

        foreach (var rate in (rates ?? Array.Empty<TaxCodeRateDto>()).OrderBy(r => r.FromDate))
            entity.AddRate(rate.FromDate, rate.ToDate, rate.Rate);
    }

    public static async Task<Result?> ValidateUniquenessAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid companyId,
        Guid? branchId,
        string code,
        Guid? excludeId,
        CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var taken = await context.TaxCodes.AnyAsync(
            t => t.TenantId == tenantId
                 && t.CompanyId == companyId
                 && t.BranchId == branchId
                 && t.Code == normalized
                 && (excludeId == null || t.Id != excludeId), ct);
        if (taken)
            return Result.Failure(ErrorCodes.TaxCodeDuplicate, "Tax code already exists for this company/branch.");
        return null;
    }
}

public sealed class CreateTaxCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateTaxCodeCommand, Result<TaxCodeDto>>
{
    public async Task<Result<TaxCodeDto>> Handle(CreateTaxCodeCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == dto.CompanyId && c.TenantId == request.TenantId, cancellationToken);
        if (!companyOk)
            return Result<TaxCodeDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        if (dto.BranchId is Guid branchId)
        {
            var branchOk = await context.Branches.AnyAsync(
                b => b.Id == branchId && b.TenantId == request.TenantId && b.CompanyId == dto.CompanyId,
                cancellationToken);
            if (!branchOk)
                return Result<TaxCodeDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");
        }

        var uniqueness = await TaxCodeGuard.ValidateUniquenessAsync(
            context, request.TenantId, dto.CompanyId, dto.BranchId, dto.Code, null, cancellationToken);
        if (uniqueness is not null)
            return Result<TaxCodeDto>.Failure(uniqueness.ErrorCode!, uniqueness.ErrorMessage ?? "Validation failed.");

        try
        {
            var next = await context.TaxCodes.Where(t => t.TenantId == request.TenantId)
                .Select(t => (int?)t.Number).MaxAsync(cancellationToken) ?? 0;

            var entity = DomainTaxCode.Create(
                request.TenantId, next + 1, dto.CompanyId, dto.Code, dto.NameAr, dto.NameEn,
                dto.BranchId, dto.AppliesTo, dto.CalculationMethod,
                dto.SalesAccountId, dto.PurchaseAccountId, dto.PriceIncludesTax, dto.IsActive);

            TaxCodeGuard.ApplyRates(context, entity, dto.Rates);
            context.TaxCodes.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return Result<TaxCodeDto>.Success(await LoadDtoAsync(context, entity.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<TaxCodeDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<TaxCodeDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var entity = await context.TaxCodes.AsNoTracking()
            .Include(t => t.Rates)
            .FirstAsync(t => t.Id == id, ct);
        return await EnrichAsync(context, entity, ct);
    }

    internal static async Task<TaxCodeDto> EnrichAsync(
        IApplicationDbContext context, DomainTaxCode entity, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == entity.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);

        string? branch = null;
        if (entity.BranchId is Guid bid)
            branch = await context.Branches.AsNoTracking()
                .Where(b => b.Id == bid).Select(b => b.NameAr).FirstOrDefaultAsync(ct);

        string? salesNo = null, salesName = null, purchaseNo = null, purchaseName = null;
        if (entity.SalesAccountId is Guid sid)
        {
            var acc = await context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.Id == sid)
                .Select(a => new { a.AccountNumber, a.NameAr })
                .FirstOrDefaultAsync(ct);
            salesNo = acc?.AccountNumber;
            salesName = acc?.NameAr;
        }

        if (entity.PurchaseAccountId is Guid pid)
        {
            var acc = await context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.Id == pid)
                .Select(a => new { a.AccountNumber, a.NameAr })
                .FirstOrDefaultAsync(ct);
            purchaseNo = acc?.AccountNumber;
            purchaseName = acc?.NameAr;
        }

        return TaxCodeMapper.ToDto(entity, company, branch, salesNo, salesName, purchaseNo, purchaseName);
    }
}

public sealed class UpdateTaxCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTaxCodeCommand, Result<TaxCodeDto>>
{
    public async Task<Result<TaxCodeDto>> Handle(UpdateTaxCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TaxCodes
            .Include(t => t.Rates)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result<TaxCodeDto>.Failure(ErrorCodes.TaxCodeNotFound, "Tax code not found.");

        var dto = request.Dto;
        if (dto.CompanyId != entity.CompanyId)
            return Result<TaxCodeDto>.Failure(ErrorCodes.RequiredField, "Company cannot be changed.");

        if (dto.BranchId is Guid branchId)
        {
            var branchOk = await context.Branches.AnyAsync(
                b => b.Id == branchId && b.TenantId == entity.TenantId && b.CompanyId == entity.CompanyId,
                cancellationToken);
            if (!branchOk)
                return Result<TaxCodeDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");
        }

        var uniqueness = await TaxCodeGuard.ValidateUniquenessAsync(
            context, entity.TenantId, entity.CompanyId, dto.BranchId, dto.Code, entity.Id, cancellationToken);
        if (uniqueness is not null)
            return Result<TaxCodeDto>.Failure(uniqueness.ErrorCode!, uniqueness.ErrorMessage ?? "Validation failed.");

        try
        {
            entity.Update(
                dto.BranchId, dto.Code, dto.NameAr, dto.NameEn, dto.AppliesTo, dto.CalculationMethod,
                dto.SalesAccountId, dto.PurchaseAccountId, dto.PriceIncludesTax, dto.IsActive);

            TaxCodeGuard.ApplyRates(context, entity, dto.Rates);

            await context.SaveChangesAsync(cancellationToken);
            return Result<TaxCodeDto>.Success(
                await CreateTaxCodeCommandHandler.LoadDtoAsync(context, entity.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<TaxCodeDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteTaxCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteTaxCodeCommand, Result>
{
    public async Task<Result> Handle(DeleteTaxCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TaxCodes
            .Include(t => t.Rates)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorCodes.TaxCodeNotFound, "Tax code not found.");

        try
        {
            entity.EnsureCanDelete();
            entity.SoftDelete(null);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
