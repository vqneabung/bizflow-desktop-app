namespace bizflow_desktop_app.Models;

/// <summary>
/// Query parameters cho GET /api/products.
/// </summary>
public record ProductSearchParams(
    string? Search = null,
    string? Category = null,
    string? SortBy = null,
    string? SortDir = null,
    int Page = 1,
    int PageSize = 20
);
