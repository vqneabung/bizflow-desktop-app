using Refit;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface IProductService
{
    [Get("/api/products")]
    Task<PaginatedResponse<ProductResponse>> GetProductsAsync([Query] ProductSearchParams? search = null);

    [Get("/api/products/{id}")]
    Task<Models.ApiResponse<ProductResponse>> GetProductAsync(string id);

    [Post("/api/products")]
    Task<Models.ApiResponse<ProductResponse>> CreateProductAsync([Body] CreateProductRequest request);

    [Put("/api/products/{id}")]
    Task<Models.ApiResponse<ProductResponse>> UpdateProductAsync(string id, [Body] UpdateProductRequest request);

    [Patch("/api/products/{id}/deactivate")]
    Task<Models.ApiResponse<object>> DeactivateProductAsync(string id);
}
