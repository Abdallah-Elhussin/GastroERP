using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class AccountClassificationMapper
{
    public static AccountClassificationDto ToDto(AccountClassification c, AccountMainClassification main) =>
        new(c.Id, c.Number, c.Code, c.NameAr, c.NameEn, c.MainClassificationId,
            main.NameAr, main.Code, main.AccountType, c.IsDefault, c.IsSystem, c.IsActive, c.SortOrder,
            c.CreatedAt, c.UpdatedAt, c.CreatedBy);
}

public sealed class CreateAccountClassificationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateAccountClassificationCommand, Result<AccountClassificationDto>>
{
    public async Task<Result<AccountClassificationDto>> Handle(
        CreateAccountClassificationCommand request, CancellationToken cancellationToken)
    {
        var main = await context.AccountMainClassifications
            .FirstOrDefaultAsync(m => m.Id == request.Dto.MainClassificationId && m.TenantId == request.TenantId, cancellationToken);
        if (main is null)
            return Result<AccountClassificationDto>.Failure(ErrorCodes.AccountMainClassificationNotFound, "Main classification not found.");

        var nameAr = request.Dto.NameAr.Trim();
        var dup = await context.AccountClassifications.AnyAsync(
            c => c.TenantId == request.TenantId
                 && c.MainClassificationId == main.Id
                 && c.NameAr == nameAr,
            cancellationToken);
        if (dup)
            return Result<AccountClassificationDto>.Failure(ErrorCodes.AccountClassificationDuplicate,
                "Classification name already exists under this main classification.");

        var nextNumber = await context.AccountClassifications
            .Where(c => c.TenantId == request.TenantId)
            .Select(c => (int?)c.Number)
            .MaxAsync(cancellationToken) ?? 0;

        try
        {
            var entity = AccountClassification.Create(
                request.TenantId, nextNumber + 1, request.Dto.Code ?? string.Empty,
                request.Dto.NameAr, request.Dto.NameEn, main.Id, nextNumber + 1);
            context.AccountClassifications.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return Result<AccountClassificationDto>.Success(AccountClassificationMapper.ToDto(entity, main));
        }
        catch (BusinessException ex)
        {
            return Result<AccountClassificationDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UpdateAccountClassificationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAccountClassificationCommand, Result<AccountClassificationDto>>
{
    public async Task<Result<AccountClassificationDto>> Handle(
        UpdateAccountClassificationCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.AccountClassifications.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result<AccountClassificationDto>.Failure(ErrorCodes.AccountClassificationNotFound, "Classification not found.");

        var main = await context.AccountMainClassifications
            .FirstOrDefaultAsync(m => m.Id == request.Dto.MainClassificationId && m.TenantId == entity.TenantId, cancellationToken);
        if (main is null)
            return Result<AccountClassificationDto>.Failure(ErrorCodes.AccountMainClassificationNotFound, "Main classification not found.");

        var nameAr = request.Dto.NameAr.Trim();
        var dup = await context.AccountClassifications.AnyAsync(
            c => c.TenantId == entity.TenantId
                 && c.MainClassificationId == main.Id
                 && c.NameAr == nameAr
                 && c.Id != entity.Id,
            cancellationToken);
        if (dup)
            return Result<AccountClassificationDto>.Failure(ErrorCodes.AccountClassificationDuplicate,
                "Classification name already exists under this main classification.");

        try
        {
            entity.Update(request.Dto.NameAr, request.Dto.NameEn, main.Id);
            context.AccountClassifications.Update(entity);
            await context.SaveChangesAsync(cancellationToken);
            return Result<AccountClassificationDto>.Success(AccountClassificationMapper.ToDto(entity, main));
        }
        catch (BusinessException ex)
        {
            return Result<AccountClassificationDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteAccountClassificationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAccountClassificationCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountClassificationCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.AccountClassifications.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorCodes.AccountClassificationNotFound, "Classification not found.");

        try { entity.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, "Default/system classifications cannot be deleted."); }

        if (await context.ChartOfAccounts.AnyAsync(a => a.AccountClassificationId == entity.Id, cancellationToken))
            return Result.Failure(ErrorCodes.AccountClassificationInUse, "Classification is used by chart of accounts.");

        var settings = await context.AccountingSettings
            .FirstOrDefaultAsync(s => s.TenantId == entity.TenantId && s.CompanyId == null, cancellationToken);
        if (settings is not null)
        {
            // Soft check: settings store account IDs not classification IDs — no direct link.
        }

        entity.SoftDelete(null);
        context.AccountClassifications.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
