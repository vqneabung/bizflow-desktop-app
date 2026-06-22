using System.IO;
using Refit;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface IOrderService
{
    [Get("/api/orders")]
    Task<PaginatedResponse<OrderSummaryResponse>> GetOrdersAsync(
        [AliasAs("page")] int page,
        [AliasAs("size")] int size,
        [AliasAs("status")] string? status = null);

    [Get("/api/orders/{id}")]
    Task<Models.ApiResponse<OrderResponse>> GetOrderAsync(string id);

    [Post("/api/orders")]
    Task<Models.ApiResponse<OrderResponse>> CreateOrderAsync([Body] CreateOrderRequest request);

    [Patch("/api/orders/{id}/cancel")]
    Task<Models.ApiResponse<OrderResponse>> CancelOrderAsync(string id, [Body] CancelOrderRequest? request);

    [Get("/api/orders/{id}/receipt")]
    Task<Stream> DownloadReceiptAsync(string id);
}
