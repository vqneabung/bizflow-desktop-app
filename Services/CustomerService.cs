using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using bizflow_desktop_app.Models;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Gọi Spring Boot Customer API qua HttpClient (qua BFF).
/// Response shape: ApiResponse<CustomerResponse> / ApiResponse<PaginatedResponse<CustomerResponse>>
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly HttpClient _http;
    private readonly ILogger<CustomerService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerService(HttpClient http, ILogger<CustomerService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<PaginatedResponse<CustomerResponse>> GetCustomersAsync(
        int page = 1, int size = 20, string? search = null)
    {
        var query = $"?page={page}&size={size}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        _logger.LogInformation("Fetching customers: {Query}", query);

        try
        {
            var response = await _http.GetAsync($"/api/customers{query}");
            response.EnsureSuccessStatusCode();

            // Backend GET list trả PaginationResponse<T> (success/data/pagination ở root), không phải ApiResponse<PaginationResponse<...>>
            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResponse<CustomerResponse>>(JsonOptions);

            return result ?? new PaginatedResponse<CustomerResponse>([], new PaginationInfo(1, 20, 0, 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch customers");
            return new PaginatedResponse<CustomerResponse>([], new PaginationInfo(1, 20, 0, 0));
        }
    }

    public async Task<CustomerResponse?> GetCustomerAsync(string id)
    {
        try
        {
            var response = await _http.GetAsync($"/api/customers/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponse<CustomerResponse>>(JsonOptions);

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch customer {Id}", id);
            return null;
        }
    }

    public async Task<CustomerResponse?> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/customers", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponse<CustomerResponse>>(JsonOptions);

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create customer");
            return null;
        }
    }

    public async Task<CustomerResponse?> UpdateCustomerAsync(string id, UpdateCustomerRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/customers/{id}", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponse<CustomerResponse>>(JsonOptions);

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update customer {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeactivateCustomerAsync(string id)
    {
        try
        {
            var response = await _http.PatchAsync($"/api/customers/{id}/deactivate", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate customer {Id}", id);
            return false;
        }
    }
}
