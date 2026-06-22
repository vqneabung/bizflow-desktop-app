using System;
using System.Text.Json.Serialization;

namespace bizflow_desktop_app.Models;

public record InventoryHistoryResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("movementType")] string MovementType,
    [property: JsonPropertyName("quantity")] decimal Quantity,
    [property: JsonPropertyName("balanceAfter")] decimal BalanceAfter,
    [property: JsonPropertyName("refType")] string RefType,
    [property: JsonPropertyName("refId")] string RefId,
    [property: JsonPropertyName("referenceNumber")] string? ReferenceNumber,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);