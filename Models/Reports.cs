using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record RevenueReportResponse(
    [property: JsonPropertyName("points")] List<RevenuePoint> Points,
    [property: JsonPropertyName("total")] decimal Total,
    [property: JsonPropertyName("periodStart")] string PeriodStart,
    [property: JsonPropertyName("periodEnd")] string PeriodEnd
);

public record RevenuePoint(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("revenue")] decimal Revenue
);

public record BestSellingReportResponse(
    [property: JsonPropertyName("products")] List<BestSellingProduct> Products
);

public record BestSellingProduct(
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("productName")] string ProductName,
    [property: JsonPropertyName("quantitySold")] decimal QuantitySold,
    [property: JsonPropertyName("revenue")] decimal Revenue
);

public record InventoryReportResponse(
    [property: JsonPropertyName("totalProducts")] long TotalProducts,
    [property: JsonPropertyName("totalValue")] decimal TotalValue,
    [property: JsonPropertyName("lowStockProducts")] List<LowStockProduct> LowStockProducts,
    [property: JsonPropertyName("byCategory")] List<CategoryCount> ByCategory
);

public record LowStockProduct(
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("productName")] string ProductName,
    [property: JsonPropertyName("stock")] decimal Stock,
    [property: JsonPropertyName("minStock")] decimal MinStock
);

public record CategoryCount(
    [property: JsonPropertyName("categoryName")] string? CategoryName,
    [property: JsonPropertyName("count")] long Count
);

public record DebtReportResponse(
    [property: JsonPropertyName("totalDebt")] decimal TotalDebt,
    [property: JsonPropertyName("customers")] List<DebtCustomer> Customers
);

public record DebtCustomer(
    [property: JsonPropertyName("customerId")] string? CustomerId,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("totalDebt")] decimal TotalDebt,
    [property: JsonPropertyName("orderCount")] long OrderCount
);
