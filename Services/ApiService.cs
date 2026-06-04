using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using bizflow_desktop_app.Models;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Gọi Spring Boot REST API qua HttpClient (typed client, DI-managed).
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, ILogger<ApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                // Parse success body: { success: true, data: { token, email, role, name } }
                var result = await response.Content
                    .ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);

                return result ?? new ApiResponse<AuthResponse>(false, "Empty response", null);
            }

            // Parse error body from Spring Boot GlobalExceptionHandler (401, etc.)
            // Returns: { success: false, message: "Sai email hoặc mật khẩu" }
            var errorBody = await TryReadErrorBody(response);
            _logger.LogWarning("Login failed (HTTP {StatusCode}): {Message}",
                (int)response.StatusCode, errorBody);

            return new ApiResponse<AuthResponse>(false, errorBody, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP connection error during login");
            return new ApiResponse<AuthResponse>(
                false, "Cannot connect to server. Please check your connection.", null);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Login request timed out");
            return new ApiResponse<AuthResponse>(
                false, "Request timed out. Please try again.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return new ApiResponse<AuthResponse>(
                false, $"Unexpected error: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Attempt to deserialize error body from non-success HTTP response.
    /// Falls back to status code text if parsing fails.
    /// </summary>
    private static async Task<string> TryReadErrorBody(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return $"HTTP {(int)response.StatusCode}";

            // Try parsing as ApiErrorResponse
            var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);
            if (errorResponse is not null && !string.IsNullOrWhiteSpace(errorResponse.Message))
                return errorResponse.Message;

            return body;
        }
        catch
        {
            return $"HTTP {(int)response.StatusCode}";
        }
    }
}
