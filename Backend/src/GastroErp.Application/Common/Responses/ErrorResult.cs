namespace GastroErp.Application.Common.Responses;

public class ErrorResult : Result
{
    public string? ExceptionMessage { get; }

    public ErrorResult(string errorCode, string errorMessage, string? exceptionMessage = null) 
        : base(false, errorCode, errorMessage)
    {
        ExceptionMessage = exceptionMessage;
    }
}
