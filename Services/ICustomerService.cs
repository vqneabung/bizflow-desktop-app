using Refit;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface ICustomerService
{
    [Get("/api/customers")]
    Task<PaginatedResponse<CustomerResponse>> GetCustomersAsync(
        [Query] int page = 1,
        [Query] int size = 20,
        [Query] string? search = null);

    [Get("/api/customers/{id}")]
    Task<Models.ApiResponse<CustomerResponse>> GetCustomerAsync(string id);

    [Post("/api/customers")]
    Task<Models.ApiResponse<CustomerResponse>> CreateCustomerAsync([Body] CreateCustomerRequest request);

    [Put("/api/customers/{id}")]
    Task<Models.ApiResponse<CustomerResponse>> UpdateCustomerAsync(string id, [Body] UpdateCustomerRequest request);

    [Patch("/api/customers/{id}/deactivate")]
    Task<Models.ApiResponse<object>> DeactivateCustomerAsync(string id);
}
