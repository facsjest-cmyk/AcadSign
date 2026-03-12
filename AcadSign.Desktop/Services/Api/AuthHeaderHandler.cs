using Microsoft.Extensions.Logging;
using AcadSign.Desktop.Services.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Api;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<AuthHeaderHandler> _logger;
    
    public AuthHeaderHandler(
        ITokenStorageService tokenStorage,
        ILogger<AuthHeaderHandler> logger)
    {
        _tokenStorage = tokenStorage;
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var (accessToken, refreshToken) = await _tokenStorage.GetTokensAsync();
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        else
        {
            _logger.LogWarning("No access token found");
        }
        
        var response = await base.SendAsync(request, cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogInformation("Access token expired, refreshing...");
        }
        
        return response;
    }
}
