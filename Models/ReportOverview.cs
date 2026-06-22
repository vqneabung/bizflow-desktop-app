namespace bizflow_desktop_app.Models;

public class ReportOverviewResponse
{
    public long TotalProducts { get; set; }
    public long OrdersThisMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public long TotalCustomers { get; set; }
    public long LowStockCount { get; set; }
}
