using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record CreateCustomerRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("notes")] string? Notes
);
