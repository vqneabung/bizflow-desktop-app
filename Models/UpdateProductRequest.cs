using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Cập nhật sản phẩm. Chỉ gửi các trường đã thay đổi.
/// API yêu cầu version (optimistic locking).
/// </summary>
public record UpdateProductRequest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("primaryUnit")] string? PrimaryUnit,
    [property: JsonPropertyName("price")] decimal? Price,
    [property: JsonPropertyName("costPrice")] decimal? CostPrice,
    [property: JsonPropertyName("stock")] int? Stock,
    [property: JsonPropertyName("minStock")] int? MinStock,
    [property: JsonPropertyName("barcode")] string? Barcode,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl
);
