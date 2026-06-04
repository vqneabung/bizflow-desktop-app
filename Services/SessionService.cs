using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Singleton — giữ JWT token trong memory.
/// Mở app lại là phải login lại.
/// </summary>
public class SessionService : ISessionService
{
    public string? Token { get; set; }
    public AuthResponse? User { get; set; }
    public bool IsAuthenticated => Token is not null;

    public void Clear()
    {
        Token = null;
        User = null;
    }
}
