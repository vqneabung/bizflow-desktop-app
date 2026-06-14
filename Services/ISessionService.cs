using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Quản lý session — JWT token + user info trong memory, persist xuống disk
/// qua ISecureTokenStore để user không phải login lại mỗi lần mở app.
/// </summary>
public interface ISessionService
{
    /// <summary>JWT access token hiện tại</summary>
    string? Token { get; }

    /// <summary>Thông tin user đã login</summary>
    AuthResponse? User { get; }

    /// <summary>Đã login chưa</summary>
    bool IsAuthenticated { get; }

    /// <summary>Đọc persisted session từ disk, populate in-memory state. Trả về true nếu thành công.</summary>
    bool Restore();

    /// <summary>Lưu session vào memory + persist xuống disk (sau login thành công).</summary>
    void Login(AuthResponse user);

    /// <summary>Xoá session khỏi memory + disk (logout).</summary>
    void Clear();
}
