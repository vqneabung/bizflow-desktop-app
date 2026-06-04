using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record PaginationInfo(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("totalPages")] int TotalPages,
    [property: JsonPropertyName("hasPrevious")] bool HasPrevious,
    [property: JsonPropertyName("hasNext")] bool HasNext
);

public record PaginatedResponse<T>(
    [property: JsonPropertyName("data")] List<T> Data,
    [property: JsonPropertyName("pagination")] PaginationInfo Pagination
);
