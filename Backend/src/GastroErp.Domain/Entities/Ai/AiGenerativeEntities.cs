using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Ai;

/// <summary>Audit log for generative AI interactions (chat, query, voice, insights)</summary>
public sealed class AiGenerativeLog : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public GenerativeInteractionType InteractionType { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string InputText { get; private set; }
    public string OutputText { get; private set; }
    public string? MetadataJson { get; private set; }
    public int TokenCount { get; private set; }
    public AiModelProvider Provider { get; private set; }

    private AiGenerativeLog() { InputText = string.Empty; OutputText = string.Empty; }

    public static AiGenerativeLog Create(
        Guid tenantId, GenerativeInteractionType type, string input, string output,
        AiModelProvider provider = AiModelProvider.Heuristic,
        Guid? userId = null, Guid? branchId = null, string? metadataJson = null, int tokenCount = 0)
        => new()
        {
            TenantId = tenantId,
            InteractionType = type,
            UserId = userId,
            BranchId = branchId,
            InputText = input,
            OutputText = output,
            Provider = provider,
            MetadataJson = metadataJson,
            TokenCount = tokenCount
        };
}

/// <summary>Voice order draft awaiting manual confirmation</summary>
public sealed class VoiceOrderDraft : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Transcript { get; private set; }
    public string ParsedItemsJson { get; private set; }
    public VoiceOrderDraftStatus Status { get; private set; }
    public decimal EstimatedTotal { get; private set; }
    public string Currency { get; private set; }

    private VoiceOrderDraft()
    {
        Transcript = string.Empty;
        ParsedItemsJson = "[]";
        Currency = "SAR";
    }

    public static VoiceOrderDraft Create(
        Guid tenantId, Guid branchId, string transcript, string parsedItemsJson,
        decimal estimatedTotal, Guid? userId = null, string currency = "SAR")
        => new()
        {
            TenantId = tenantId,
            BranchId = branchId,
            UserId = userId,
            Transcript = transcript,
            ParsedItemsJson = parsedItemsJson,
            EstimatedTotal = estimatedTotal,
            Currency = currency,
            Status = VoiceOrderDraftStatus.Draft
        };

    public void Confirm() => Status = VoiceOrderDraftStatus.Confirmed;
    public void Cancel() => Status = VoiceOrderDraftStatus.Cancelled;
}
