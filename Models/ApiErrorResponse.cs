using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Error response from Spring Boot GlobalExceptionHandler.
/// Returned as JSON when an API call fails (401, 403, 500, etc.).
/// </summary>
public record ApiErrorResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message
);
