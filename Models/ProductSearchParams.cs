using Refit;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Query parameters cho GET /api/products.
/// </summary>
public record ProductSearchParams(
    [property: AliasAs("search")] string? Search = null,
    [property: AliasAs("category")] string? Category = null,
    [property: AliasAs("sortBy")] string? SortBy = null,
    [property: AliasAs("sortDir")] string? SortDir = null,
    [property: AliasAs("page")] int Page = 1,
    [property: AliasAs("size")] int PageSize = 20
);
