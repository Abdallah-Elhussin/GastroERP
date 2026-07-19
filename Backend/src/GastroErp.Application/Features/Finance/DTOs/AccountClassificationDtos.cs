using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Finance.DTOs;

public record AccountMainClassificationDto(
    Guid Id, string Code, string NameAr, string NameEn, AccountType AccountType, int SortOrder, bool IsActive);

public record AccountClassificationDto(
    Guid Id, int Number, string Code, string NameAr, string NameEn,
    Guid MainClassificationId, string MainClassificationNameAr, string MainClassificationCode,
    AccountType AccountType, bool IsDefault, bool IsSystem, bool IsActive, int SortOrder,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, string? CreatedBy);

public record CreateAccountClassificationDto(string NameAr, string NameEn, Guid MainClassificationId, string? Code = null);

public record UpdateAccountClassificationDto(string NameAr, string NameEn, Guid MainClassificationId);

public record AccountClassificationFilterDto(string? Search = null, Guid? MainClassificationId = null);
