namespace GastroErp.Presentation.Contracts.Responses;

public record ApiErrorResponse
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public object? Details { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}
