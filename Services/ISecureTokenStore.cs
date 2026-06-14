using bizflow_desktop_app.Models;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Securely persists the JWT token + user info to disk so the user stays
/// logged in across app restarts. The actual crypto provider is injected
/// (see DataProtectionTokenStore for the Microsoft.AspNetCore.DataProtection
/// implementation).
/// </summary>
public interface ISecureTokenStore
{
    /// <summary>Returns the persisted session, or null if none / corrupted.</summary>
    StoredSession? Read();

    /// <summary>Encrypts and writes the session to disk.</summary>
    void Write(AuthResponse user);

    /// <summary>Removes the persisted session (logout).</summary>
    void Clear();
}

/// <summary>
/// Plain DTO for what we persist. AuthResponse already contains token + user,
/// so we just wrap it for clarity at the storage boundary.
/// </summary>
public record StoredSession(string Token, AuthResponse User);
