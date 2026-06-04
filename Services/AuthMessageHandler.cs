using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using bizflow_desktop_app.Models;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.Services;

/// <summary>
/// DelegatingHandler tự động inject Bearer token từ ISessionService vào mọi HTTP request.
/// Cũng log request/response ở Debug level.
/// </summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly ISessionService _session;
    private readonly ILogger<AuthMessageHandler> _logger;

    public AuthMessageHandler(ISessionService session, ILogger<AuthMessageHandler> logger)
    {
        _session = session;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Inject Bearer token nếu đã đăng nhập
        if (_session.Token is not null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _session.Token);
        }

        _logger.LogDebug("→ {Method} {Url}", request.Method, request.RequestUri);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogDebug("← {StatusCode} {Method} {Url}",
                (int)response.StatusCode, request.Method, request.RequestUri);

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP request failed: {Method} {Url}",
                request.Method, request.RequestUri);
            throw;
        }
    }
}
