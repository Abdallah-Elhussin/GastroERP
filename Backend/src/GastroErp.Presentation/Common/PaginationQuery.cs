namespace GastroErp.Presentation.Common;

public record PaginationQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null,
    string SortDirection = "asc",
    Dictionary<string, string>? Filters = null
);
