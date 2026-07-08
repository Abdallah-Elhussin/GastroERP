using System.Collections.Generic;

namespace GastroErp.Application.Common.Responses;

public record ValidationError(string PropertyName, string ErrorMessage, string? ErrorCode = null);

public interface IValidationResult
{
    IReadOnlyCollection<ValidationError> Errors { get; }
}

public class ValidationResult : Result, IValidationResult
{
    public IReadOnlyCollection<ValidationError> Errors { get; }

    private ValidationResult(IReadOnlyCollection<ValidationError> errors) 
        : base(false, "ValidationFailed", "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public static ValidationResult WithErrors(IEnumerable<ValidationError> errors) => new(new List<ValidationError>(errors).AsReadOnly());
}

public class ValidationResult<T> : Result<T>, IValidationResult
{
    public IReadOnlyCollection<ValidationError> Errors { get; }

    private ValidationResult(IReadOnlyCollection<ValidationError> errors) 
        : base(false, "ValidationFailed", "One or more validation errors occurred.", default)
    {
        Errors = errors;
    }

    public static ValidationResult<T> WithErrors(IEnumerable<ValidationError> errors) => new(new List<ValidationError>(errors).AsReadOnly());
}
