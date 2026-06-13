using System;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record CustomerResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("totalDebt")] decimal TotalDebt,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt
);
