using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.DTOs;

public record AiChatRequestDto(
    string Message, string Language = "ar", Guid? BranchId = null, string? Context = null);

public record AiChatResponseDto(
    string Reply, AiModelProvider Provider, int TokenCount, Guid LogId);

public record AiInsightFilterDto(Guid? BranchId = null, string Language = "ar", int Days = 7);

public record AiDashboardInsightDto(
    string Summary, string Language,
    IReadOnlyList<string> Highlights, IReadOnlyList<string> Alerts,
    DateTimeOffset GeneratedAt);

public record NaturalLanguageQueryDto(string Question, string Language = "ar", Guid? BranchId = null);

public record NaturalLanguageQueryResultDto(
    string Question, string Answer, string QueryType, object? Data,
    AiModelProvider Provider, Guid LogId);

public record VoiceOrderRequestDto(
    string Transcript, Guid BranchId, string Language = "ar");

public record VoiceOrderLineDto(Guid ProductId, string ProductName, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public record VoiceOrderDraftDto(
    Guid Id, string Transcript, VoiceOrderDraftStatus Status,
    IReadOnlyList<VoiceOrderLineDto> Items, decimal EstimatedTotal, string Currency,
    bool RequiresConfirmation);

public record ConfirmVoiceOrderDto(Guid DraftId);
