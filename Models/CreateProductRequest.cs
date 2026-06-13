using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record CreateProductRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("categoryId")] string? CategoryId,
    [property: JsonPropertyName("primaryUnitId")] string PrimaryUnitId,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("costPrice")] decimal? CostPrice,
    [property: JsonPropertyName("stock")] decimal Stock,
    [property: JsonPropertyName("minStock")] decimal? MinStock,
    [property: JsonPropertyName("barcode")] string? Barcode,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("imageKeys")] List<string>? ImageKeys
);
