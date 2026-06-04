using System.Threading.Tasks;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface IProductService
{
    Task<PaginatedResponse<ProductResponse>> GetProductsAsync(ProductSearchParams? search = null);
    Task<ProductResponse?> GetProductAsync(string id);
    Task<ProductResponse?> CreateProductAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> DeactivateProductAsync(string id);
}
