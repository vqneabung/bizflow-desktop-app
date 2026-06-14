using System;
using bizflow_desktop_app.Models;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.Services;

/// <summary>
/// In-memory session cache + auto-persist to disk via ISecureTokenStore.
///
/// On startup, App.axaml.cs calls <see cref="Restore"/> to rehydrate from disk
/// (if a session was persisted from a previous run). On <see cref="Login"/>, the
/// session is written to disk. On <see cref="Clear"/> (logout), the disk file
/// is removed.
/// </summary>
public class SessionService : ISessionService
{
    private readonly ISecureTokenStore _store;
    private readonly ILogger<SessionService> _logger;

    public string? Token { get; set; }
    public AuthResponse? User { get; set; }
    public bool IsAuthenticated => Token is not null;

    public SessionService(ISecureTokenStore store, ILogger<SessionService> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Read persisted session from disk and populate in-memory state.
    /// Returns true if a session was restored, false otherwise.
    /// </summary>
    public bool Restore()
    {
        var stored = _store.Read();
        if (stored is null)
            return false;

        Token = stored.Token;
        User = stored.User;
        _logger.LogInformation("Session restored for {Email}", User?.Email);
        return true;
    }

    /// <summary>
    /// Persist the current session to disk (after successful login).
    /// </summary>
    public void Login(AuthResponse user)
    {
        User = user;
        Token = user.Token;
        _store.Write(user);
        _logger.LogInformation("Session persisted for {Email}", user.Email);
    }

    public void Clear()
    {
        var wasAuthenticated = IsAuthenticated;
        Token = null;
        User = null;
        _store.Clear();
        if (wasAuthenticated)
            _logger.LogInformation("Session cleared");
    }
}
