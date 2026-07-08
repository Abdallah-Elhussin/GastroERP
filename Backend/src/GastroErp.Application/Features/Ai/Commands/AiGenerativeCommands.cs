using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public record AiChatCommand(Guid TenantId, AiChatRequestDto Request, Guid? UserId = null)
    : IRequest<Result<AiChatResponseDto>>;

public record NaturalLanguageQueryCommand(Guid TenantId, NaturalLanguageQueryDto Request, Guid? UserId = null)
    : IRequest<Result<NaturalLanguageQueryResultDto>>;

public record ParseVoiceOrderCommand(Guid TenantId, VoiceOrderRequestDto Request, Guid? UserId = null)
    : IRequest<Result<VoiceOrderDraftDto>>;

public record ConfirmVoiceOrderCommand(Guid TenantId, ConfirmVoiceOrderDto Dto, Guid? UserId = null)
    : IRequest<Result<VoiceOrderDraftDto>>;
