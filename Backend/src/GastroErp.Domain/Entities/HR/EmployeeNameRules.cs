using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.HR;

public static class EmployeeNameRules
{
    public static bool IsTripleName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3;
    }

    public static void EnsureTripleName(string name)
    {
        if (!IsTripleName(name))
            throw new BusinessException(ErrorCodes.InvalidTripleName, "Employee name must contain at least three parts.");
    }

    public static void EnsureOptionalTripleName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!IsTripleName(name))
            throw new BusinessException(ErrorCodes.InvalidTripleName, "Arabic name must contain at least three parts.");
    }
}
