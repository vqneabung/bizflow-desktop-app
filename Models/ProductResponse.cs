using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record ProductResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("categoryId")] string? CategoryId,
    [property: JsonPropertyName("categoryName")] string? CategoryName,
    [property: JsonPropertyName("primaryUnitId")] string PrimaryUnitId,
    [property: JsonPropertyName("primaryUnitName")] string PrimaryUnitName,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("costPrice")] decimal? CostPrice,
    [property: JsonPropertyName("stock")] decimal Stock,
    [property: JsonPropertyName("minStock")] decimal MinStock,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("imageKeys")] List<string> ImageKeys,
    [property: JsonPropertyName("barcode")] string? Barcode,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("isLowStock")] bool IsLowStock,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt
);
