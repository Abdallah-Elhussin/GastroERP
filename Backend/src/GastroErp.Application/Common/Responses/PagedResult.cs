using System.Collections.Generic;

namespace GastroErp.Application.Common.Responses;

public class PagedResult<T> : Result<IReadOnlyCollection<T>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    private PagedResult(bool isSuccess, string? errorCode, string? errorMessage, IReadOnlyCollection<T>? data, int pageNumber, int pageSize, int totalCount, int totalPages)
        : base(isSuccess, errorCode, errorMessage, data)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }

    public static PagedResult<T> Success(IReadOnlyCollection<T> data, int pageNumber, int pageSize, int totalCount)
    {
        var totalPages = totalCount > 0 ? (int)System.Math.Ceiling(totalCount / (double)pageSize) : 0;
        return new PagedResult<T>(true, null, null, data, pageNumber, pageSize, totalCount, totalPages);
    }
}
