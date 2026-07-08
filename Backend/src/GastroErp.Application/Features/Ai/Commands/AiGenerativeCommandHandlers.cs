using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public sealed class AiChatCommandHandler : IRequestHandler<AiChatCommand, Result<AiChatResponseDto>>
{
    private readonly IManagementAiAssistantService _service;
    public AiChatCommandHandler(IManagementAiAssistantService service) => _service = service;
    public async Task<Result<AiChatResponseDto>> Handle(AiChatCommand request, CancellationToken ct)
        => Result<AiChatResponseDto>.Success(await _service.ChatAsync(request.TenantId, request.Request, request.UserId, ct));
}

public sealed class NaturalLanguageQueryCommandHandler : IRequestHandler<NaturalLanguageQueryCommand, Result<NaturalLanguageQueryResultDto>>
{
    private readonly INaturalLanguageQueryService _service;
    public NaturalLanguageQueryCommandHandler(INaturalLanguageQueryService service) => _service = service;
    public async Task<Result<NaturalLanguageQueryResultDto>> Handle(NaturalLanguageQueryCommand request, CancellationToken ct)
        => Result<NaturalLanguageQueryResultDto>.Success(await _service.QueryAsync(request.TenantId, request.Request, request.UserId, ct));
}

public sealed class ParseVoiceOrderCommandHandler : IRequestHandler<ParseVoiceOrderCommand, Result<VoiceOrderDraftDto>>
{
    private readonly IVoiceOrderingService _service;
    public ParseVoiceOrderCommandHandler(IVoiceOrderingService service) => _service = service;
    public async Task<Result<VoiceOrderDraftDto>> Handle(ParseVoiceOrderCommand request, CancellationToken ct)
        => Result<VoiceOrderDraftDto>.Success(await _service.ParseVoiceOrderAsync(request.TenantId, request.Request, request.UserId, ct));
}

public sealed class ConfirmVoiceOrderCommandHandler : IRequestHandler<ConfirmVoiceOrderCommand, Result<VoiceOrderDraftDto>>
{
    private readonly IVoiceOrderingService _service;
    public ConfirmVoiceOrderCommandHandler(IVoiceOrderingService service) => _service = service;
    public async Task<Result<VoiceOrderDraftDto>> Handle(ConfirmVoiceOrderCommand request, CancellationToken ct)
        => Result<VoiceOrderDraftDto>.Success(await _service.ConfirmDraftAsync(request.TenantId, request.Dto, request.UserId, ct));
}
