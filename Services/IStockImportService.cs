using Refit;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface IStockImportService
{
    [Get("/api/stock-imports")]
    Task<PaginatedResponse<StockImportSummaryResponse>> GetStockImportsAsync(
        [AliasAs("page")] int page,
        [AliasAs("size")] int size);

    [Get("/api/stock-imports/{id}")]
    Task<Models.ApiResponse<StockImportResponse>> GetStockImportAsync(string id);

    [Post("/api/stock-imports")]
    Task<Models.ApiResponse<StockImportResponse>> CreateStockImportAsync([Body] CreateStockImportRequest request);
}
