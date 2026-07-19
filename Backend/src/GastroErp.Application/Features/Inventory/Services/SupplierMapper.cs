using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

public static class SupplierMapper
{
    public static SupplierListItemDto ToListItem(
        Supplier s,
        string? accountNumber,
        decimal currentBalance,
        DateOnly? lastPurchase,
        DateOnly? lastPayment) => new(
        s.Id,
        s.Code,
        s.NameAr,
        s.NameEn,
        accountNumber,
        s.ApAccountId,
        s.Category,
        s.City,
        s.Country,
        s.TaxNumber,
        s.CreditLimit,
        currentBalance,
        lastPurchase,
        lastPayment,
        s.IsActive,
        s.IsBlacklisted,
        s.IsOverCreditLimit(currentBalance));

    public static SupplierDto ToDto(
        Supplier s,
        string? accountNumber = null,
        decimal currentBalance = 0,
        SupplierDashboardDto? dashboard = null,
        SupplierStatsDto? stats = null) => new(
        s.Id,
        s.TenantId,
        s.CompanyId,
        s.BranchId,
        s.Code,
        s.NameAr,
        s.NameEn,
        s.SupplierType,
        s.Category,
        s.TaxNumber,
        s.CommercialRegister,
        s.EstablishmentNumber,
        s.TaxRegistrationCountry,
        s.TaxType,
        s.DefaultTaxPercent,
        s.TaxCertificateExpiry,
        s.CommercialRegisterExpiry,
        s.TaxCertificatePath,
        s.CommercialRegisterPath,
        s.ContactPerson,
        s.ContactJobTitle,
        s.Phone,
        s.Mobile,
        s.Email,
        s.Website,
        s.City,
        s.Region,
        s.Country,
        s.PostalCode,
        s.Address,
        s.ApAccountId,
        accountNumber,
        s.DiscountAccountId,
        s.PurchaseReturnAccountId,
        s.ExchangeDifferenceAccountId,
        s.Currency,
        s.DefaultPaymentMethod,
        s.PaymentDueDays,
        s.PaymentTerms,
        s.CreditLimit,
        s.OpeningBalance,
        s.OpeningBalanceDate,
        s.VatEvaluation,
        s.LeadTimeDays,
        s.IsPreferred,
        s.Rating,
        s.IsActive,
        s.IsBlacklisted,
        s.BlacklistReason,
        s.Notes,
        currentBalance,
        s.IsOverCreditLimit(currentBalance),
        s.Contacts.Count,
        s.CreatedAt,
        s.Contacts.Select(c => new SupplierContactDto(
            c.Id, c.NameAr, c.NameEn, c.PhoneNumber, c.Mobile, c.Email, c.Position)).ToList(),
        s.PaymentMethods.Select(p => new SupplierPaymentMethodDto(
            p.Id, p.Kind, p.BankName, p.Iban, p.Swift, p.AccountNumber,
            p.BeneficiaryName, p.Currency, p.IsDefault, p.Notes)).ToList(),
        s.Attachments.Select(a => new SupplierAttachmentDto(
            a.Id, a.FileName, a.ContentType, a.StoragePath, a.SizeBytes, a.Category, a.CreatedAt)).ToList(),
        dashboard,
        stats);

    public static async Task<decimal> ComputeCurrentBalanceAsync(
        IQueryable<PurchaseInvoice> invoices,
        Guid supplierId,
        decimal openingBalance,
        CancellationToken ct)
    {
        var outstanding = await invoices
            .Where(i => i.SupplierId == supplierId && i.Status == PurchasingDocumentStatus.Posted)
            .SumAsync(i => i.TotalAmount - i.PaidAmount, ct);
        return openingBalance + outstanding;
    }

    public static List<string> BuildWarnings(Supplier s, decimal currentBalance, DateOnly today)
    {
        var warnings = new List<string>();
        if (s.IsBlacklisted) warnings.Add("SupplierBlacklisted");
        if (!s.IsActive) warnings.Add("SupplierInactive");
        if (s.IsOverCreditLimit(currentBalance)) warnings.Add("CreditLimitExceeded");
        if (s.IsTaxCertificateExpired(today)) warnings.Add("TaxCertificateExpired");
        if (s.IsCommercialRegisterExpired(today)) warnings.Add("CommercialRegisterExpired");
        if (s.ApAccountId is null) warnings.Add("ApAccountMissing");
        return warnings;
    }
}
