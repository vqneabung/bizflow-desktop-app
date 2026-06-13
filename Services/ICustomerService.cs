using System.Threading.Tasks;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface ICustomerService
{
    Task<PaginatedResponse<CustomerResponse>> GetCustomersAsync(int page = 1, int size = 20, string? search = null);
    Task<CustomerResponse?> GetCustomerAsync(string id);
    Task<CustomerResponse?> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> UpdateCustomerAsync(string id, UpdateCustomerRequest request);
    Task<bool> DeactivateCustomerAsync(string id);
}
