using System;
using System.IO;
using System.Text;
using System.Text.Json;
using bizflow_desktop_app.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Persists the session to disk using Microsoft.AspNetCore.DataProtection encryption.
///
/// Storage layout (per user):
///   %LOCALAPPDATA%\Bizflow\
///     Keys\            ← DataProtection key ring (DPAPI on Windows, keyring on Linux/macOS)
///     session.dat      ← encrypted JSON: { "t": "JWT", "u": { "email", "role", "name" } }
///
/// Why DataProtection over raw DPAPI:
///   - Cross-platform (Windows DPAPI + macOS Keychain + Linux libsecret via SecretService)
///   - "Purpose string" isolates Bizflow keys from other apps sharing the same key ring
///   - Key rotation supported out of the box
/// </summary>
public class DataProtectionTokenStore : ISecureTokenStore
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionTokenStore> _logger;
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public DataProtectionTokenStore(
        IDataProtectionProvider provider,
        ILogger<DataProtectionTokenStore> logger)
    {
        // "Purpose" scopes this protector so only code with the same string can unprotect.
        _protector = provider.CreateProtector("Bizflow.Auth.Session.v1");
        _logger = logger;
        _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Bizflow",
            "session.dat");
    }

    public StoredSession? Read()
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var protectedBytes = File.ReadAllBytes(_filePath);
            var plainBytes = _protector.Unprotect(protectedBytes);
            var json = Encoding.UTF8.GetString(plainBytes);

            var dto = JsonSerializer.Deserialize<SessionDto>(json, JsonOptions);
            if (dto is null || string.IsNullOrEmpty(dto.Token) || dto.User is null)
            {
                _logger.LogWarning("Session file present but payload was empty or malformed; treating as no session");
                Clear();
                return null;
            }

            return new StoredSession(dto.Token, dto.User);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
        {
            _logger.LogWarning(ex, "Failed to read persisted session; user will need to log in again");
            Clear();
            return null;
        }
    }

    public void Write(AuthResponse user)
    {
        try
        {
            var dto = new SessionDto(user.Token, user);
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            var plainBytes = Encoding.UTF8.GetBytes(json);
            var protectedBytes = _protector.Protect(plainBytes);

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllBytes(_filePath, protectedBytes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
        {
            _logger.LogError(ex, "Failed to persist session; user will need to log in again on next launch");
        }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Failed to delete session file at {Path}", _filePath);
        }
    }

    /// <summary>
    /// Internal wire format. The token is the JWT, user is the same AuthResponse
    /// the backend returned at login.
    /// </summary>
    private record SessionDto(string Token, AuthResponse User);
}
