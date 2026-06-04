namespace bizflow_desktop_app.Models;

/// <summary>
/// DTO gửi đến POST /api/auth/login
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);
