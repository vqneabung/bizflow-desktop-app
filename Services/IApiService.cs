using System.Threading.Tasks;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

/// <summary>
/// HTTP client gọi Spring Boot API.
/// </summary>
public interface IApiService
{
    /// <summary>
    /// POST /api/auth/login — đăng nhập, nhận JWT.
    /// </summary>
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// POST /api/auth/logout — đăng xuất, thu hồi session phía BFF.
    /// </summary>
    Task LogoutAsync();
}
