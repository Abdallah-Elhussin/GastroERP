using System.Collections.Generic;

namespace GastroErp.Application.Common.Pagination;

public class SearchCriteria
{
    public string? Keyword { get; set; }
    public Dictionary<string, string> Filters { get; set; } = new();
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
