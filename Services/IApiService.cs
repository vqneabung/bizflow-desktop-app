using Refit;
using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

public interface IApiService
{
    [Post("/api/auth/login")]
    Task<Models.ApiResponse<AuthResponse>> LoginAsync([Body] LoginRequest request);

    [Post("/api/auth/logout")]
    Task<Models.ApiResponse<object>> LogoutAsync();
}
