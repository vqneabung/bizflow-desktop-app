using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Quản lý session — lưu JWT token + user info trong memory.
/// </summary>
public interface ISessionService
{
    /// <summary>JWT access token hiện tại</summary>
    string? Token { get; set; }

    /// <summary>Thông tin user đã login</summary>
    AuthResponse? User { get; set; }

    /// <summary>Đã login chưa</summary>
    bool IsAuthenticated { get; }

    /// <summary>Xoá session (logout)</summary>
    void Clear();
}
