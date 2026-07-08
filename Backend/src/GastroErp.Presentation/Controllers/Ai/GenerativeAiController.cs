using Asp.Versioning;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Application.Features.Ai.Services;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Ai;

[ApiVersion("1.0")]
public class GenerativeAiController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpPost(ApiRoutes.Ai.Chat)]
    [HasPermission(Permissions.Ai.ChatUse)]
    public async Task<IActionResult> Chat([FromBody] AiChatRequestDto dto)
        => HandleResult(await Mediator.Send(new AiChatCommand(TenantId, dto, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Ai.Chat}/stream")]
    [HasPermission(Permissions.Ai.ChatUse)]
    public async Task StreamChat([FromBody] AiChatRequestDto dto, CancellationToken ct)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IManagementAiAssistantService>();
        Response.ContentType = "text/event-stream";
        await foreach (var chunk in service.ChatStreamAsync(TenantId, dto, CurrentUserId, ct))
        {
            await Response.WriteAsync($"data: {chunk}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }

    [HttpGet($"{ApiRoutes.Ai.Insights}/dashboard")]
    [HasPermission(Permissions.Ai.ChatUse)]
    public async Task<IActionResult> GetDashboardInsights([FromQuery] AiInsightFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDashboardInsightsQuery(TenantId, filter)));

    [HttpPost(ApiRoutes.Ai.Query)]
    [HasPermission(Permissions.Ai.ChatUse)]
    public async Task<IActionResult> Query([FromBody] NaturalLanguageQueryDto dto)
        => HandleResult(await Mediator.Send(new NaturalLanguageQueryCommand(TenantId, dto, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Ai.Voice}/order")]
    [HasPermission(Permissions.Ai.VoiceUse)]
    public async Task<IActionResult> ParseVoiceOrder([FromBody] VoiceOrderRequestDto dto)
        => HandleResult(await Mediator.Send(new ParseVoiceOrderCommand(TenantId, dto, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Ai.Voice}/order/confirm")]
    [HasPermission(Permissions.Ai.VoiceUse)]
    public async Task<IActionResult> ConfirmVoiceOrder([FromBody] ConfirmVoiceOrderDto dto)
        => HandleResult(await Mediator.Send(new ConfirmVoiceOrderCommand(TenantId, dto, CurrentUserId)));
}
