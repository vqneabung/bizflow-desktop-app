using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Wrapper response chung từ Spring Boot API.
/// Cấu trúc: { "success": true, "message": "...", "data": { ... } }
/// </summary>
public record ApiResponse<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] T? Data
);
