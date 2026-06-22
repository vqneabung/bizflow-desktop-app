using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record OrderResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("ownerId")] string OwnerId,
    [property: JsonPropertyName("customerId")] string? CustomerId,
    [property: JsonPropertyName("referenceNumber")] string ReferenceNumber,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("paidAmount")] decimal PaidAmount,
    [property: JsonPropertyName("debtAmount")] decimal DebtAmount,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("itemCount")] int ItemCount,
    [property: JsonPropertyName("items")] List<OrderItemResponse> Items,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt
);

public record OrderSummaryResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("customerId")] string? CustomerId,
    [property: JsonPropertyName("referenceNumber")] string ReferenceNumber,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("paidAmount")] decimal PaidAmount,
    [property: JsonPropertyName("debtAmount")] decimal DebtAmount,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("itemCount")] int ItemCount,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt
);

public record OrderItemResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("productName")] string ProductName,
    [property: JsonPropertyName("quantity")] decimal Quantity,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
    [property: JsonPropertyName("subtotal")] decimal Subtotal
);

public record CreateOrderRequest(
    [property: JsonPropertyName("customerId")] string? CustomerId,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("items")] List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("quantity")] decimal Quantity,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice
);

public record CancelOrderRequest(
    [property: JsonPropertyName("notes")] string? Notes
);
