using GastroErp.Domain.Entities.Finance;

namespace GastroErp.Application.Features.Finance.DTOs;

public record TaxRegistrationCertificateDto(
    Guid Id,
    int Version,
    string FileName,
    string StoragePath,
    string? ContentType,
    string? DocumentNumber,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    string? Notes,
    bool IsCurrent,
    DateTimeOffset UploadedAt);

public record UpsertTaxRegistrationDto(
    Guid CompanyId,
    string VatNumber,
    Guid? BranchId = null,
    string? BranchVatNumber = null,
    string? TaxOffice = null,
    TaxpayerType TaxpayerType = TaxpayerType.Company,
    string? ActivityCode = null,
    string? ActivityNameAr = null,
    string? ActivityNameEn = null,
    decimal DefaultTaxRate = 15m,
    DateOnly? RegistrationDate = null,
    DateOnly? ExpiryDate = null,
    TaxRegistrationStatus Status = TaxRegistrationStatus.Active,
    string? Notes = null,
    int SortOrder = 0,
    string? CertificateDocumentNumber = null,
    DateOnly? CertificateIssueDate = null,
    DateOnly? CertificateExpiryDate = null,
    string? CertificateNotes = null);

public record TaxRegistrationProfileDto(
    Guid Id,
    int Number,
    Guid CompanyId,
    string? CompanyNameAr,
    Guid? BranchId,
    string? BranchNameAr,
    string VatNumber,
    string? BranchVatNumber,
    string? TaxOffice,
    TaxpayerType TaxpayerType,
    string? ActivityCode,
    string? ActivityNameAr,
    string? ActivityNameEn,
    decimal DefaultTaxRate,
    DateOnly? RegistrationDate,
    DateOnly? ExpiryDate,
    TaxRegistrationStatus Status,
    string? Notes,
    bool IsSystem,
    int SortOrder,
    bool HasBeenUsed,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    TaxRegistrationCertificateDto? CurrentCertificate,
    IReadOnlyList<TaxRegistrationCertificateDto> Certificates);

public record TaxRegistrationFilterDto(
    Guid? CompanyId = null,
    Guid? BranchId = null,
    TaxRegistrationStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);
