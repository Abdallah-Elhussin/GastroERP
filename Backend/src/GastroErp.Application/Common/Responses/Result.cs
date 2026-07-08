using System;

namespace GastroErp.Application.Common.Responses;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorCode, string? errorMessage)
    {
        if (isSuccess && errorCode != null)
            throw new InvalidOperationException("Cannot provide an error code for a successful result.");
        
        if (!isSuccess && string.IsNullOrWhiteSpace(errorCode))
            throw new InvalidOperationException("Cannot provide a successful result without an error code.");

        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string errorCode, string? errorMessage = null) => new(false, errorCode, errorMessage);
}

public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool isSuccess, string? errorCode, string? errorMessage, T? data) 
        : base(isSuccess, errorCode, errorMessage)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, null, null, data);
    public static new Result<T> Failure(string errorCode, string? errorMessage = null) => new(false, errorCode, errorMessage, default);
}
