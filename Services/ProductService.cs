using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using bizflow_desktop_app.Models;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Gọi Spring Boot Product API qua HttpClient.
/// </summary>
public class ProductService : IProductService
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductService(HttpClient http, ILogger<ProductService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductResponse>> GetProductsAsync(ProductSearchParams? search = null)
    {
        var query = BuildQuery(search ?? new ProductSearchParams());
        _logger.LogInformation("Fetching products: {Query}", query);

        try
        {
            var response = await _http.GetAsync($"/api/products{query}");
            response.EnsureSuccessStatusCode();

            // Backend GET list trả PaginationResponse<T> (success/data/pagination ở root), không phải ApiResponse<PaginationResponse<...>>
            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResponse<ProductResponse>>(JsonOptions);

            return result ?? new PaginatedResponse<ProductResponse>([], new PaginationInfo(1, 20, 0, 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch products");
            return new PaginatedResponse<ProductResponse>([], new PaginationInfo(1, 20, 0, 0));
        }
    }

    public async Task<ProductResponse?> GetProductAsync(string id)
    {
        try
        {
            var response = await _http.GetAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponse<ProductResponse>>(JsonOptions);

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch product {Id}", id);
            return null;
        }
    }

    public async Task<ProductResponse?> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/products", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponse<ProductResponse>>(JsonOptions);

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            return null;
        }
    }

    public async Task<ProductResponse?> UpdateProductAsync(UpdateProductRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/products/{request.Id}", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponse<ProductResponse>>(JsonOptions);

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {Id}", request.Id);
            return null;
        }
    }

    public async Task<bool> DeactivateProductAsync(string id)
    {
        try
        {
            var response = await _http.PatchAsync($"/api/products/{id}/deactivate", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate product {Id}", id);
            return false;
        }
    }

    private static string BuildQuery(ProductSearchParams p)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrWhiteSpace(p.Search))
            parts.Add($"search={Uri.EscapeDataString(p.Search)}");
        // Note: category filter disabled — backend now expects categoryId (UUID), not category (name)
        // TODO: load categories from /api/reference/categories and map name→UUID
        if (!string.IsNullOrWhiteSpace(p.SortBy))
        {
            parts.Add($"sortBy={p.SortBy}");
            parts.Add($"sortDir={p.SortDir ?? "asc"}");
        }
        parts.Add($"page={p.Page}");
        parts.Add($"size={p.PageSize}");

        return "?" + string.Join("&", parts);
    }
}
