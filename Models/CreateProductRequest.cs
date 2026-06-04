using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record CreateProductRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("primaryUnit")] string? PrimaryUnit,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("costPrice")] decimal? CostPrice,
    [property: JsonPropertyName("stock")] int Stock,
    [property: JsonPropertyName("minStock")] int? MinStock,
    [property: JsonPropertyName("barcode")] string? Barcode,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl
);
