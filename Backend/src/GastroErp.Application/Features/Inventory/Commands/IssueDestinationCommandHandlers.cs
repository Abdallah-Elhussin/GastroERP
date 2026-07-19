using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Issuing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

internal static class IssueDestinationMapper
{
    public static async Task<IssueDestinationDto> ToDtoAsync(
        IApplicationDbContext db,
        IssueDestination dest,
        CancellationToken ct)
    {
        string? ccName = null;
        if (dest.DefaultCostCenterId.HasValue)
        {
            ccName = await db.CostCenters.AsNoTracking()
                .Where(c => c.Id == dest.DefaultCostCenterId.Value)
                .Select(c => c.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        string? glName = null;
        if (dest.DefaultGlAccountId.HasValue)
        {
            glName = await db.ChartOfAccounts.AsNoTracking()
                .Where(a => a.Id == dest.DefaultGlAccountId.Value)
                .Select(a => a.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        return new IssueDestinationDto(
            dest.Id,
            dest.TenantId,
            dest.Code,
            dest.NameAr,
            dest.NameEn,
            dest.Description,
            dest.DestinationType.ToString(),
            (byte)dest.DestinationType,
            dest.DefaultGlAccountId,
            glName,
            dest.DefaultCostCenterId,
            ccName,
            dest.AllowChangeAccountOnIssue,
            dest.RequireEmployee,
            dest.RequireProject,
            dest.RequireCostCenter,
            dest.RequireBranch,
            dest.RequireReason,
            dest.RequireApproval,
            dest.AllowDirectIssue,
            dest.AllowNegativeStock,
            dest.WorkflowDefinitionId,
            dest.BuildPolicySummary(),
            dest.SortOrder,
            dest.IsSystem,
            dest.IsActive);
    }
}

public sealed class CreateIssueDestinationCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateIssueDestinationCommandHandler> logger)
    : IRequestHandler<CreateIssueDestinationCommand, Result<IssueDestinationDto>>
{
    public async Task<Result<IssueDestinationDto>> Handle(CreateIssueDestinationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        try
        {
            var code = dto.Code.Trim().ToUpperInvariant();
            if (await context.IssueDestinations.AnyAsync(
                    d => d.TenantId == dto.TenantId && d.Code == code, cancellationToken))
                return Result<IssueDestinationDto>.Failure("DuplicateCode", "Issue destination code already exists.");

            if (await context.IssueDestinations.AnyAsync(
                    d => d.TenantId == dto.TenantId && d.NameAr == dto.NameAr.Trim(), cancellationToken))
                return Result<IssueDestinationDto>.Failure("DuplicateNameAr", "Arabic name already exists.");

            var dest = new IssueDestination(
                dto.TenantId,
                code,
                dto.NameAr,
                (IssueDestinationType)dto.DestinationType,
                dto.NameEn,
                dto.Description,
                dto.DefaultGlAccountId,
                dto.DefaultCostCenterId,
                dto.AllowChangeAccountOnIssue,
                dto.RequireEmployee,
                dto.RequireProject,
                dto.RequireCostCenter,
                dto.RequireBranch,
                dto.RequireReason,
                dto.RequireApproval,
                dto.AllowDirectIssue,
                dto.AllowNegativeStock,
                dto.WorkflowDefinitionId,
                dto.SortOrder);

            if (!dto.IsActive) dest.Deactivate();

            context.IssueDestinations.Add(dest);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("IssueDestination created: {Id} ({Code})", dest.Id, dest.Code);
            return Result<IssueDestinationDto>.Success(await IssueDestinationMapper.ToDtoAsync(context, dest, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<IssueDestinationDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class UpdateIssueDestinationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateIssueDestinationCommand, Result<IssueDestinationDto>>
{
    public async Task<Result<IssueDestinationDto>> Handle(UpdateIssueDestinationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var dest = await context.IssueDestinations
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == dto.TenantId, cancellationToken);
        if (dest is null)
            return Result<IssueDestinationDto>.Failure("IssueDestinationNotFound", "Issue destination not found.");

        try
        {
            if (await context.IssueDestinations.AnyAsync(
                    d => d.TenantId == dto.TenantId && d.Id != dest.Id && d.NameAr == dto.NameAr.Trim(), cancellationToken))
                return Result<IssueDestinationDto>.Failure("DuplicateNameAr", "Arabic name already exists.");

            dest.UpdateGeneral(dto.NameAr, dto.NameEn, dto.Description, (IssueDestinationType)dto.DestinationType, dto.SortOrder);
            dest.UpdateAccounting(dto.DefaultGlAccountId, dto.DefaultCostCenterId, dto.AllowChangeAccountOnIssue);
            dest.UpdatePolicies(
                dto.RequireEmployee, dto.RequireProject, dto.RequireCostCenter, dto.RequireBranch,
                dto.RequireReason, dto.RequireApproval, dto.AllowDirectIssue, dto.AllowNegativeStock);
            dest.SetWorkflow(dto.WorkflowDefinitionId);

            if (dto.IsActive) dest.Activate();
            else dest.Deactivate();

            await context.SaveChangesAsync(cancellationToken);
            return Result<IssueDestinationDto>.Success(await IssueDestinationMapper.ToDtoAsync(context, dest, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<IssueDestinationDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class DeleteIssueDestinationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteIssueDestinationCommand, Result>
{
    public async Task<Result> Handle(DeleteIssueDestinationCommand request, CancellationToken cancellationToken)
    {
        var dest = await context.IssueDestinations
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == request.TenantId, cancellationToken);
        if (dest is null) return Result.Failure("IssueDestinationNotFound", "Issue destination not found.");

        var inUse = await context.GoodsIssues.AnyAsync(
            g => g.TenantId == request.TenantId && g.IssueDestinationId == dest.Id, cancellationToken);
        if (inUse)
            return Result.Failure("InUse", "Issue destination is used by goods issue documents. Deactivate it instead.");

        try
        {
            dest.SoftDeleteDestination(null);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateIssueDestinationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateIssueDestinationCommand, Result>
{
    public async Task<Result> Handle(ActivateIssueDestinationCommand request, CancellationToken cancellationToken)
    {
        var dest = await context.IssueDestinations
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == request.TenantId, cancellationToken);
        if (dest is null) return Result.Failure("IssueDestinationNotFound", "Issue destination not found.");
        dest.Activate();
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateIssueDestinationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateIssueDestinationCommand, Result>
{
    public async Task<Result> Handle(DeactivateIssueDestinationCommand request, CancellationToken cancellationToken)
    {
        var dest = await context.IssueDestinations
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == request.TenantId, cancellationToken);
        if (dest is null) return Result.Failure("IssueDestinationNotFound", "Issue destination not found.");
        dest.Deactivate();
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
