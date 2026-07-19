using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainReason = GastroErp.Domain.Entities.Finance.NotificationReason;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class NotificationReasonMapper
{
    public static NotificationReasonDto ToDto(
        DomainReason entity, string? accountNumber, string? accountNameAr)
        => new(
            entity.Id,
            entity.Number,
            entity.Code,
            entity.NameAr,
            entity.NameEn,
            entity.NoteType,
            entity.PartyType,
            entity.CounterpartAccountId,
            accountNumber,
            accountNameAr,
            entity.UsesTax,
            entity.IsActive,
            entity.HasBeenUsed,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy);
}

internal static class NotificationReasonGuard
{
    /// <summary>
    /// Expected AccountClassification codes for party types (seeded COA classifications).
    /// </summary>
    public static string? ExpectedClassificationCode(NotificationPartyType partyType) => partyType switch
    {
        NotificationPartyType.Customer => "receivable",
        NotificationPartyType.Supplier => "payable",
        NotificationPartyType.Employee => "salaries_payable",
        _ => null
    };

    public static async Task<Result?> ValidateAccountAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid accountId,
        NotificationPartyType partyType,
        CancellationToken ct)
    {
        var account = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.Id == accountId && a.TenantId == tenantId)
            .Select(a => new
            {
                a.Id,
                a.IsPostingAllowed,
                a.IsSummaryAccount,
                a.IsActive,
                a.AccountClassificationId
            })
            .FirstOrDefaultAsync(ct);

        if (account is null)
            return Result.Failure(ErrorCodes.RequiredField, "Counterpart account is required.");

        if (!account.IsActive || account.IsSummaryAccount || !account.IsPostingAllowed)
            return Result.Failure(ErrorCodes.NotificationReasonAccountInvalid,
                "Counterpart account must be an active posting account.");

        var expectedCode = ExpectedClassificationCode(partyType);
        if (expectedCode is null || account.AccountClassificationId is null)
            return null;

        var classificationCode = await context.AccountClassifications.AsNoTracking()
            .Where(c => c.Id == account.AccountClassificationId)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(ct);

        if (classificationCode is not null
            && !string.Equals(classificationCode, expectedCode, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(ErrorCodes.NotificationReasonAccountPartyMismatch,
                $"Counterpart account classification must match party type ({expectedCode}).");
        }

        return null;
    }

    public static async Task<Result?> ValidateUniquenessAsync(
        IApplicationDbContext context,
        Guid tenantId,
        string code,
        Guid? excludeId,
        CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var taken = await context.NotificationReasons.AnyAsync(
            r => r.TenantId == tenantId
                 && r.Code == normalized
                 && (excludeId == null || r.Id != excludeId), ct);
        if (taken)
            return Result.Failure(ErrorCodes.NotificationReasonDuplicate, "Reason code already exists.");
        return null;
    }
}

public sealed class CreateNotificationReasonCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateNotificationReasonCommand, Result<NotificationReasonDto>>
{
    public async Task<Result<NotificationReasonDto>> Handle(
        CreateNotificationReasonCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var uniqueness = await NotificationReasonGuard.ValidateUniquenessAsync(
            context, request.TenantId, dto.Code, null, cancellationToken);
        if (uniqueness is not null)
            return Result<NotificationReasonDto>.Failure(
                uniqueness.ErrorCode!, uniqueness.ErrorMessage ?? "Validation failed.");

        var accountCheck = await NotificationReasonGuard.ValidateAccountAsync(
            context, request.TenantId, dto.CounterpartAccountId, dto.PartyType, cancellationToken);
        if (accountCheck is not null)
            return Result<NotificationReasonDto>.Failure(
                accountCheck.ErrorCode!, accountCheck.ErrorMessage ?? "Validation failed.");

        try
        {
            var next = await context.NotificationReasons.Where(r => r.TenantId == request.TenantId)
                .Select(r => (int?)r.Number).MaxAsync(cancellationToken) ?? 0;

            var entity = DomainReason.Create(
                request.TenantId, next + 1, dto.Code, dto.NameAr, dto.NoteType, dto.PartyType,
                dto.CounterpartAccountId, dto.NameEn, dto.UsesTax, dto.IsActive);

            context.NotificationReasons.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return Result<NotificationReasonDto>.Success(
                await LoadDtoAsync(context, entity.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<NotificationReasonDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<NotificationReasonDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var entity = await context.NotificationReasons.AsNoTracking()
            .FirstAsync(r => r.Id == id, ct);
        return await EnrichAsync(context, entity, ct);
    }

    internal static async Task<NotificationReasonDto> EnrichAsync(
        IApplicationDbContext context, DomainReason entity, CancellationToken ct)
    {
        var acc = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.Id == entity.CounterpartAccountId)
            .Select(a => new { a.AccountNumber, a.NameAr })
            .FirstOrDefaultAsync(ct);
        return NotificationReasonMapper.ToDto(entity, acc?.AccountNumber, acc?.NameAr);
    }
}

public sealed class UpdateNotificationReasonCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateNotificationReasonCommand, Result<NotificationReasonDto>>
{
    public async Task<Result<NotificationReasonDto>> Handle(
        UpdateNotificationReasonCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.NotificationReasons
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result<NotificationReasonDto>.Failure(
                ErrorCodes.NotificationReasonNotFound, "Notification reason not found.");

        var dto = request.Dto;
        var uniqueness = await NotificationReasonGuard.ValidateUniquenessAsync(
            context, entity.TenantId, dto.Code, entity.Id, cancellationToken);
        if (uniqueness is not null)
            return Result<NotificationReasonDto>.Failure(
                uniqueness.ErrorCode!, uniqueness.ErrorMessage ?? "Validation failed.");

        var accountCheck = await NotificationReasonGuard.ValidateAccountAsync(
            context, entity.TenantId, dto.CounterpartAccountId, dto.PartyType, cancellationToken);
        if (accountCheck is not null)
            return Result<NotificationReasonDto>.Failure(
                accountCheck.ErrorCode!, accountCheck.ErrorMessage ?? "Validation failed.");

        try
        {
            entity.Update(
                dto.Code, dto.NameAr, dto.NameEn, dto.NoteType, dto.PartyType,
                dto.CounterpartAccountId, dto.UsesTax, dto.IsActive);
            await context.SaveChangesAsync(cancellationToken);
            return Result<NotificationReasonDto>.Success(
                await CreateNotificationReasonCommandHandler.LoadDtoAsync(
                    context, entity.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<NotificationReasonDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteNotificationReasonCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteNotificationReasonCommand, Result>
{
    public async Task<Result> Handle(
        DeleteNotificationReasonCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.NotificationReasons
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorCodes.NotificationReasonNotFound, "Notification reason not found.");

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
