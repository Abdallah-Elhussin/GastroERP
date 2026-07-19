using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainDocumentType = GastroErp.Domain.Entities.Finance.DocumentType;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class DocumentTypeMapper
{
    public static string ModuleCode(DocumentModule m) => m switch
    {
        DocumentModule.Inventory => "Inventory",
        DocumentModule.Purchasing => "Purchasing",
        DocumentModule.Sales => "Sales",
        DocumentModule.Finance => "Finance",
        DocumentModule.Hr => "HR",
        DocumentModule.Production => "Production",
        DocumentModule.Maintenance => "Maintenance",
        DocumentModule.Pos => "POS",
        _ => "General"
    };

    public static DocumentTypeDto ToDto(DomainDocumentType d) =>
        new(
            d.Id, d.Code, d.NameAr, d.NameEn, d.Description, d.Module, ModuleCode(d.Module),
            d.Prefix, d.Suffix, d.StartingNumber, d.LastNumber, d.NumberLength,
            d.ResetYearly, d.ResetMonthly, d.NumberPerBranch, d.NumberPerCompany,
            d.ApprovalMode, d.RequiresApproval, d.UsesWorkflow, d.WorkflowDefinitionId,
            d.PostingMode, d.AutoPost, d.PostAfterApproval,
            d.AffectsInventory, d.AffectsCost, d.AffectsAccounting, d.AffectsCash,
            d.AffectsCustomers, d.AffectsSuppliers, d.AffectsAssets, d.AffectsPayroll,
            d.AllowCreate, d.AllowUpdate, d.AllowApprove, d.AllowPost, d.AllowCancel, d.AllowDelete,
            d.AllowAttachments, d.AllowPrint, d.AllowEditAfterSave, d.AllowDeleteDocuments,
            d.AllowCancelDocuments, d.AllowCopy, d.AllowReopen, d.ShowInReports, d.ShowInDashboard,
            d.IsSystem, d.IsActive, d.SortOrder,
            d.LifecycleStages.OrderBy(s => s.SortOrder)
                .Select(s => new DocumentTypeLifecycleStageDto(s.Code, s.NameAr, s.NameEn, s.SortOrder, s.IsTerminal))
                .ToList());

    public static void ApplyDto(DomainDocumentType doc, UpsertDocumentTypeDto dto, bool allowIdentityChange)
    {
        if (allowIdentityChange)
            doc.UpdateIdentity(dto.Code, dto.Module);

        doc.UpdateBasic(dto.NameAr, dto.NameEn, dto.Description, dto.SortOrder);
        doc.UpdateNumbering(
            dto.Prefix, dto.Suffix, dto.StartingNumber,
            dto.LastNumber > 0 ? dto.LastNumber : Math.Max(0, dto.StartingNumber - 1),
            dto.NumberLength, dto.ResetYearly, dto.ResetMonthly, dto.NumberPerBranch, dto.NumberPerCompany);
        doc.UpdateApproval(dto.ApprovalMode, dto.RequiresApproval, dto.UsesWorkflow, dto.WorkflowDefinitionId);
        doc.UpdatePosting(dto.PostingMode, dto.AutoPost, dto.PostAfterApproval);
        doc.UpdateImpact(dto.AffectsInventory, dto.AffectsCost, dto.AffectsAccounting, dto.AffectsCash,
            dto.AffectsCustomers, dto.AffectsSuppliers, dto.AffectsAssets, dto.AffectsPayroll);
        doc.UpdateCapabilities(dto.AllowCreate, dto.AllowUpdate, dto.AllowApprove, dto.AllowPost, dto.AllowCancel, dto.AllowDelete);
        doc.UpdateExtras(dto.AllowAttachments, dto.AllowPrint, dto.AllowEditAfterSave, dto.AllowDeleteDocuments,
            dto.AllowCancelDocuments, dto.AllowCopy, dto.AllowReopen, dto.ShowInReports, dto.ShowInDashboard);

        if (dto.LifecycleStages is { Count: > 0 })
        {
            doc.ReplaceLifecycleStages(dto.LifecycleStages.Select(s =>
                (s.Code, s.NameAr, s.NameEn, s.SortOrder, s.IsTerminal)));
        }

        if (dto.IsActive) doc.Activate();
        else doc.Deactivate();
    }
}

public sealed class CreateDocumentTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateDocumentTypeCommand, Result<DocumentTypeDto>>
{
    public async Task<Result<DocumentTypeDto>> Handle(CreateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var code = request.Dto.Code.Trim().ToUpperInvariant();
        if (await context.DocumentTypes.AnyAsync(d => d.TenantId == request.TenantId && d.Code == code, cancellationToken))
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeCodeDuplicate, "Document type code already exists.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.DocumentTypes.AnyAsync(d => d.TenantId == request.TenantId && d.NameAr == nameAr, cancellationToken))
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeNameDuplicate, "Arabic name already exists.");

        try
        {
            var doc = DomainDocumentType.Create(
                request.TenantId, code, nameAr, request.Dto.NameEn, request.Dto.Module,
                request.Dto.Prefix, request.Dto.Description, request.Dto.Suffix,
                request.Dto.StartingNumber, request.Dto.NumberLength, request.Dto.SortOrder);

            DocumentTypeMapper.ApplyDto(doc, request.Dto with { Code = code }, allowIdentityChange: true);
            context.DocumentTypes.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            return Result<DocumentTypeDto>.Success(DocumentTypeMapper.ToDto(doc));
        }
        catch (BusinessException ex)
        {
            return Result<DocumentTypeDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UpdateDocumentTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDocumentTypeCommand, Result<DocumentTypeDto>>
{
    public async Task<Result<DocumentTypeDto>> Handle(UpdateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.DocumentTypes
            .Include(d => d.LifecycleStages)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeNotFound, "Document type not found.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.DocumentTypes.AnyAsync(
                d => d.TenantId == doc.TenantId && d.NameAr == nameAr && d.Id != doc.Id, cancellationToken))
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeNameDuplicate, "Arabic name already exists.");

        var inUse = await DocumentTypeUsageGuard.IsInUseAsync(context, doc, cancellationToken);
        var newCode = request.Dto.Code.Trim().ToUpperInvariant();
        if (inUse && !string.Equals(newCode, doc.Code, StringComparison.Ordinal))
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeCodeLocked, "Code cannot change after use.");
        if (inUse && request.Dto.Module != doc.Module)
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeModuleLocked, "Module cannot change after use.");

        if (!string.Equals(newCode, doc.Code, StringComparison.Ordinal)
            && await context.DocumentTypes.AnyAsync(d => d.TenantId == doc.TenantId && d.Code == newCode, cancellationToken))
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeCodeDuplicate, "Document type code already exists.");

        try
        {
            // Code/module changes only when not in use — create path sets code; update keeps existing if locked
            DocumentTypeMapper.ApplyDto(doc, request.Dto, allowIdentityChange: !inUse);
            context.DocumentTypes.Update(doc);
            await context.SaveChangesAsync(cancellationToken);
            return Result<DocumentTypeDto>.Success(DocumentTypeMapper.ToDto(doc));
        }
        catch (BusinessException ex)
        {
            return Result<DocumentTypeDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateDocumentTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateDocumentTypeCommand, Result>
{
    public async Task<Result> Handle(ActivateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.DocumentTypes.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure(ErrorCodes.DocumentTypeNotFound, "Document type not found.");
        doc.Activate();
        context.DocumentTypes.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateDocumentTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateDocumentTypeCommand, Result>
{
    public async Task<Result> Handle(DeactivateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.DocumentTypes.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure(ErrorCodes.DocumentTypeNotFound, "Document type not found.");
        doc.Deactivate();
        context.DocumentTypes.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteDocumentTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDocumentTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.DocumentTypes
            .Include(d => d.LifecycleStages)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure(ErrorCodes.DocumentTypeNotFound, "Document type not found.");

        try { doc.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        if (await DocumentTypeUsageGuard.IsInUseAsync(context, doc, cancellationToken))
            return Result.Failure(ErrorCodes.DocumentTypeInUse, "Document type is in use. Deactivate it instead.");

        doc.SoftDelete(null);
        context.DocumentTypes.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class CopyDocumentTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CopyDocumentTypeCommand, Result<DocumentTypeDto>>
{
    public async Task<Result<DocumentTypeDto>> Handle(CopyDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var source = await context.DocumentTypes
            .Include(d => d.LifecycleStages)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (source is null)
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeNotFound, "Document type not found.");

        var code = request.Dto.NewCode.Trim().ToUpperInvariant();
        if (await context.DocumentTypes.AnyAsync(d => d.TenantId == source.TenantId && d.Code == code, cancellationToken))
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeCodeDuplicate, "Document type code already exists.");

        try
        {
            // Re-load tracked for CloneAs which needs stages in memory — recreate from DTO path
            var tracked = await context.DocumentTypes
                .Include(d => d.LifecycleStages)
                .FirstAsync(d => d.Id == request.Id, cancellationToken);

            var clone = tracked.CloneAs(code, request.Dto.NameAr, request.Dto.NameEn, request.Dto.Prefix);
            context.DocumentTypes.Add(clone);
            await context.SaveChangesAsync(cancellationToken);
            return Result<DocumentTypeDto>.Success(DocumentTypeMapper.ToDto(clone));
        }
        catch (BusinessException ex)
        {
            return Result<DocumentTypeDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

internal static class DocumentTypeUsageGuard
{
    /// <summary>
    /// يعتبر النوع مستخدماً إذا زاد LastNumber عن البداية أو وُجدت مستندات مخزون مرتبطة بنفس البادئة تقريبياً.
    /// </summary>
    public static async Task<bool> IsInUseAsync(IApplicationDbContext context, DomainDocumentType doc, CancellationToken ct)
    {
        if (doc.LastNumber >= doc.StartingNumber)
            return true;

        var prefix = doc.Prefix;
        if (await context.GoodsIssues.AnyAsync(g => g.IssueNumber.StartsWith(prefix), ct))
            return true;
        if (await context.OpeningBalances.AnyAsync(o => o.DocumentNumber.StartsWith(prefix), ct))
            return true;
        if (await context.StockTransfers.AnyAsync(t => t.TransferNumber.StartsWith(prefix), ct))
            return true;
        if (await context.JournalEntries.AnyAsync(j => j.EntryNumber.StartsWith(prefix), ct))
            return true;

        return false;
    }
}
