using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

public class CreateSupplierCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateSupplierCommandHandler> logger)
    : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var nameExists = await context.Suppliers.AsNoTracking()
            .AnyAsync(s => s.TenantId == dto.TenantId
                           && s.CompanyId == dto.CompanyId
                           && s.NameAr == dto.NameAr.Trim(), cancellationToken);
        if (nameExists)
            return Result<SupplierDto>.Failure("DuplicateName", "Supplier name already exists for this company.");

        if (!string.IsNullOrWhiteSpace(dto.TaxNumber))
        {
            var taxExists = await context.Suppliers.AsNoTracking()
                .AnyAsync(s => s.TenantId == dto.TenantId && s.TaxNumber == dto.TaxNumber.Trim(), cancellationToken);
            if (taxExists)
                return Result<SupplierDto>.Failure("DuplicateTaxNumber", "Tax number already exists.");
        }

        var accountExists = await context.ChartOfAccounts.AsNoTracking()
            .AnyAsync(a => a.Id == dto.ApAccountId, cancellationToken);
        if (!accountExists)
            return Result<SupplierDto>.Failure("AccountNotFound", "AP account not found.");

        var code = string.IsNullOrWhiteSpace(dto.Code)
            ? await GenerateCodeAsync(dto.TenantId, cancellationToken)
            : dto.Code.Trim().ToUpperInvariant();

        if (await context.Suppliers.AsNoTracking().AnyAsync(s => s.TenantId == dto.TenantId && s.Code == code, cancellationToken))
            return Result<SupplierDto>.Failure("DuplicateCode", "Supplier code already exists.");

        try
        {
            var supplier = Supplier.Create(
                dto.TenantId, code, dto.NameAr, dto.NameEn, dto.Currency,
                dto.CompanyId, dto.BranchId, dto.SupplierType, dto.Category,
                dto.ApAccountId, dto.DefaultPaymentMethod);

            supplier.UpdateFinancial(
                dto.ApAccountId, null, null, null, dto.Currency, dto.DefaultPaymentMethod,
                dto.PaymentDueDays, null, dto.CreditLimit, 0, null,
                SupplierVatEvaluation.ExcludeVat, 0);

            supplier.UpdateContact(null, null, dto.Phone, null, dto.Email, null, dto.City, null, dto.Country, null, dto.Address);
            if (!string.IsNullOrWhiteSpace(dto.TaxNumber))
                supplier.UpdateTax(dto.TaxNumber, null, null, null, null, 0, null, null);
            if (!string.IsNullOrWhiteSpace(dto.Notes))
                supplier.UpdateNotes(dto.Notes);

            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Supplier created: {Id} {Code}", supplier.Id, supplier.Code);
            return Result<SupplierDto>.Success(SupplierMapper.ToDto(supplier));
        }
        catch (BusinessException ex)
        {
            return Result<SupplierDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    private async Task<string> GenerateCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var count = await context.Suppliers.IgnoreQueryFilters()
            .CountAsync(s => s.TenantId == tenantId, ct);
        return $"SUP-{(count + 1):D5}";
    }
}

public class UpsertSupplierMasterCommandHandler(
    IApplicationDbContext context,
    ILogger<UpsertSupplierMasterCommandHandler> logger)
    : IRequestHandler<UpsertSupplierMasterCommand, Result<SupplierDto>>
{
    public async Task<Result<SupplierDto>> Handle(UpsertSupplierMasterCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers
            .Include(s => s.Contacts)
            .Include(s => s.PaymentMethods)
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier is null)
            return Result<SupplierDto>.Failure("SupplierNotFound", "Supplier not found.");

        var dto = request.Dto;
        var nameExists = await context.Suppliers.AsNoTracking()
            .AnyAsync(s => s.TenantId == supplier.TenantId
                           && s.CompanyId == dto.CompanyId
                           && s.NameAr == dto.NameAr.Trim()
                           && s.Id != supplier.Id, cancellationToken);
        if (nameExists)
            return Result<SupplierDto>.Failure("DuplicateName", "Supplier name already exists for this company.");

        if (!string.IsNullOrWhiteSpace(dto.TaxNumber))
        {
            var taxExists = await context.Suppliers.AsNoTracking()
                .AnyAsync(s => s.TenantId == supplier.TenantId
                               && s.TaxNumber == dto.TaxNumber.Trim()
                               && s.Id != supplier.Id, cancellationToken);
            if (taxExists)
                return Result<SupplierDto>.Failure("DuplicateTaxNumber", "Tax number already exists.");
        }

        try
        {
            supplier.UpdateBasic(dto.NameAr, dto.NameEn, dto.SupplierType, dto.Category, dto.CompanyId, dto.BranchId);
            supplier.UpdateTax(
                dto.TaxNumber, dto.CommercialRegister, dto.EstablishmentNumber,
                dto.TaxRegistrationCountry, dto.TaxType, dto.DefaultTaxPercent,
                dto.TaxCertificateExpiry, dto.CommercialRegisterExpiry);
            supplier.UpdateContact(
                dto.ContactPerson, dto.ContactJobTitle, dto.Phone, dto.Mobile, dto.Email, dto.Website,
                dto.City, dto.Region, dto.Country, dto.PostalCode, dto.Address);
            supplier.UpdateFinancial(
                dto.ApAccountId, dto.DiscountAccountId, dto.PurchaseReturnAccountId, dto.ExchangeDifferenceAccountId,
                dto.Currency, dto.DefaultPaymentMethod, dto.PaymentDueDays, dto.PaymentTerms,
                dto.CreditLimit, dto.OpeningBalance, dto.OpeningBalanceDate, dto.VatEvaluation, dto.LeadTimeDays);
            supplier.SetPreferred(dto.IsPreferred);
            supplier.SetRating(dto.Rating);
            supplier.UpdateNotes(dto.Notes);

            if (dto.PaymentMethods is not null)
            {
                supplier.ReplacePaymentMethods(dto.PaymentMethods.Select(p => new SupplierPaymentMethodDraft(
                    p.Kind, p.BankName, p.Iban, p.Swift, p.AccountNumber, p.BeneficiaryName,
                    p.Currency, p.IsDefault, p.Notes)));
            }

            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Supplier master updated: {Id}", supplier.Id);
            return Result<SupplierDto>.Success(SupplierMapper.ToDto(supplier));
        }
        catch (BusinessException ex)
        {
            return Result<SupplierDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class DeleteSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSupplierCommand, Result>
{
    public async Task<Result> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier is null)
            return Result.Failure("SupplierNotFound", "Supplier not found.");

        var hasPo = await context.PurchaseOrders.AsNoTracking().AnyAsync(p => p.SupplierId == request.Id, cancellationToken);
        var hasGrn = await context.GoodsReceipts.AsNoTracking().AnyAsync(g => g.SupplierId == request.Id, cancellationToken);
        var hasInv = await context.PurchaseInvoices.AsNoTracking().AnyAsync(i => i.SupplierId == request.Id, cancellationToken);
        var hasRet = await context.PurchaseReturns.AsNoTracking().AnyAsync(r => r.SupplierId == request.Id, cancellationToken);
        if (hasPo || hasGrn || hasInv || hasRet)
            return Result.Failure("SupplierInUse", "Cannot delete a supplier with purchasing documents.");

        supplier.SoftDelete(null);
        context.Suppliers.Update(supplier);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class BlacklistSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<BlacklistSupplierCommand, Result>
{
    public async Task<Result> Handle(BlacklistSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier is null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.Blacklist(request.Reason);
        context.Suppliers.Update(supplier);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ClearSupplierBlacklistCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ClearSupplierBlacklistCommand, Result>
{
    public async Task<Result> Handle(ClearSupplierBlacklistCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier is null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.ClearBlacklist();
        context.Suppliers.Update(supplier);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetSupplierDefaultPaymentMethodCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetSupplierDefaultPaymentMethodCommand, Result>
{
    public async Task<Result> Handle(SetSupplierDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.Include(s => s.PaymentMethods)
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        if (supplier is null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        try
        {
            supplier.SetDefaultPaymentMethod(request.PaymentMethodId);
            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class RemoveSupplierPaymentMethodCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RemoveSupplierPaymentMethodCommand, Result>
{
    public async Task<Result> Handle(RemoveSupplierPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.Include(s => s.PaymentMethods)
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        if (supplier is null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        try
        {
            supplier.RemovePaymentMethod(request.PaymentMethodId);
            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class RemoveSupplierAttachmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RemoveSupplierAttachmentCommand, Result>
{
    public async Task<Result> Handle(RemoveSupplierAttachmentCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        if (supplier is null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        try
        {
            supplier.RemoveAttachment(request.AttachmentId);
            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
