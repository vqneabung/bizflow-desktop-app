using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

/// <summary>
/// DTO nhận từ POST /api/auth/login (trường data)
/// </summary>
public record AuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("name")] string? Name
);
