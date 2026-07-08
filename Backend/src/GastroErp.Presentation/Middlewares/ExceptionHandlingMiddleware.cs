using GastroErp.Domain.Common.Exceptions;
using GastroErp.Presentation.Contracts.Responses;
using System.Text.Json;

namespace GastroErp.Presentation.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var code = "InternalServerError";
        var message = "An error occurred while processing your request.";
        object? details = null;

        switch (exception)
        {
            case BusinessException businessEx:
                statusCode = StatusCodes.Status422UnprocessableEntity;
                code = businessEx.ErrorCode;
                message = businessEx.Message; // Should be localized by the time it gets here, or localized in middleware if ILocalizationService is used.
                break;
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                code = "Unauthorized";
                message = "You are not authorized to perform this action.";
                break;
            case KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                code = "NotFound";
                message = "The requested resource was not found.";
                break;
                // ValidationException is handled by FluentValidation filter, but we could handle it here too.
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var traceId = context.TraceIdentifier;
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? traceId;

        var response = new ApiErrorResponse
        {
            Code = code,
            Message = message,
            Details = details,
            TraceId = traceId,
            CorrelationId = correlationId
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
