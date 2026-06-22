using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record StockImportResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("ownerId")] string OwnerId,
    [property: JsonPropertyName("referenceNumber")] string ReferenceNumber,
    [property: JsonPropertyName("supplier")] string? Supplier,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("importDate")] DateTime ImportDate,
    [property: JsonPropertyName("totalCost")] decimal TotalCost,
    [property: JsonPropertyName("itemCount")] int ItemCount,
    [property: JsonPropertyName("items")] List<StockImportItemResponse> Items,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt
);

public record StockImportSummaryResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("referenceNumber")] string ReferenceNumber,
    [property: JsonPropertyName("supplier")] string? Supplier,
    [property: JsonPropertyName("importDate")] DateTime ImportDate,
    [property: JsonPropertyName("totalCost")] decimal TotalCost,
    [property: JsonPropertyName("itemCount")] int ItemCount,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record StockImportItemResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("productName")] string ProductName,
    [property: JsonPropertyName("quantity")] decimal Quantity,
    [property: JsonPropertyName("unitCost")] decimal UnitCost,
    [property: JsonPropertyName("subtotal")] decimal Subtotal
);

public record CreateStockImportRequest(
    [property: JsonPropertyName("referenceNumber")] string? ReferenceNumber,
    [property: JsonPropertyName("supplier")] string? Supplier,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("importDate")] DateTime? ImportDate,
    [property: JsonPropertyName("items")] List<CreateStockImportItemRequest> Items
);

public record CreateStockImportItemRequest(
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("quantity")] decimal Quantity,
    [property: JsonPropertyName("unitCost")] decimal UnitCost
);
