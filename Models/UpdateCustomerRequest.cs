using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Cập nhật khách hàng. Tất cả fields đều optional.
/// </summary>
public record UpdateCustomerRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("notes")] string? Notes
);
