using System.ComponentModel.DataAnnotations;

namespace bizflow_desktop_app.Models;

/// <summary>
/// Strongly-typed settings mapped from appsettings.json section "ApiSettings".
/// Validated at startup via ValidateDataAnnotations.
/// </summary>
public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    /// <summary>Spring Boot API base URL (e.g. http://localhost:8080)</summary>
    [Required, Url]
    public string BaseUrl { get; init; } = "http://localhost:8080";

    /// <summary>HTTP request timeout in seconds</summary>
    [Range(5, 120)]
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>Number of transient-failure retries</summary>
    [Range(0, 10)]
    public int RetryCount { get; init; } = 2;
}
