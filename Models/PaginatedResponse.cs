using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Pagination metadata matching backend PaginationMeta.
/// page = 1-based, size = page size, totalElements = total rows, totalPages = computed.
/// </summary>
public record PaginationInfo(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("totalElements")] long TotalElements,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

public record PaginatedResponse<T>(
    [property: JsonPropertyName("data")] List<T> Data,
    [property: JsonPropertyName("pagination")] PaginationInfo Pagination
);
