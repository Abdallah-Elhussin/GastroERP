using System;

namespace GastroErp.Domain.Common.Exceptions;

/// <summary>
/// استثناء خاص بقواعد العمل (Business Rules Validation).
/// </summary>
public sealed class BusinessException : Exception
{
    public string ErrorCode { get; }
    public object[]? Args { get; }

    public BusinessException(string errorCode, params object[]? args) 
        : base($"Business exception occurred: {errorCode}")
    {
        ErrorCode = errorCode;
        Args = args;
    }
}
