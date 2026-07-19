using GastroErp.Domain.Entities.Finance;

namespace GastroErp.Application.Features.Finance.DTOs;

public record UpsertNotificationReasonDto(
    string Code,
    string NameAr,
    NotificationNoteType NoteType,
    NotificationPartyType PartyType,
    Guid CounterpartAccountId,
    string? NameEn = null,
    bool UsesTax = false,
    bool IsActive = true);

public record NotificationReasonDto(
    Guid Id,
    int Number,
    string Code,
    string NameAr,
    string? NameEn,
    NotificationNoteType NoteType,
    NotificationPartyType PartyType,
    Guid CounterpartAccountId,
    string? AccountNumber,
    string? AccountNameAr,
    bool UsesTax,
    bool IsActive,
    bool HasBeenUsed,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy);

public record NotificationReasonFilterDto(
    NotificationNoteType? NoteType = null,
    NotificationPartyType? PartyType = null,
    bool? IsActive = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);
