using Refit;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface IReportService
{
    [Get("/api/reports/overview")]
    Task<Models.ApiResponse<ReportOverviewResponse>> GetOverviewAsync();

    [Get("/api/reports/revenue")]
    Task<Models.ApiResponse<RevenueReportResponse>> GetRevenueAsync([AliasAs("range")] string range);

    [Get("/api/reports/best-selling")]
    Task<Models.ApiResponse<BestSellingReportResponse>> GetBestSellingAsync([AliasAs("limit")] int limit = 10);

    [Get("/api/reports/inventory")]
    Task<Models.ApiResponse<InventoryReportResponse>> GetInventoryAsync();

    [Get("/api/reports/debt")]
    Task<Models.ApiResponse<DebtReportResponse>> GetDebtAsync();
}
