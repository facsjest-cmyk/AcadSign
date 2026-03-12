using System.Net.Http.Headers;
using AcadSign.Backend.Application.Common.Exceptions;
using AcadSign.Backend.Application.Interfaces;
using AcadSign.Backend.Application.Models;
using AcadSign.Backend.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AcadSign.Backend.Infrastructure.Services;

public class SisAttestationExportClient : ISisAttestationExportClient
{
    private const string CorrelationIdItemKey = "CorrelationId";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SisAttestationExportClient> _logger;
    private readonly SisAttestationExportParser _parser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SisAttestationExportClient(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SisAttestationExportClient> logger,
        SisAttestationExportParser parser,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _parser = parser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SisAttestationExportResult> GetStudentsAsync(CancellationToken cancellationToken = default)
    {
        var url = _configuration["Sis:AttestationExportUrl"]
                  ?? "http://10.2.22.201/api/v1/admin/attestation/export";

        var timeoutSeconds = TryGetInt("Sis:TimeoutSeconds", 10);
        var retryCount = TryGetInt("Sis:RetryCount", 2);

        var correlationId = GetCorrelationId();

        Exception? lastException = null;

        for (var attempt = 1; attempt <= Math.Max(1, retryCount + 1); attempt++)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    var status = (int)response.StatusCode;
                    _logger.LogError("SIS export failed (HTTP {Status}) correlationId={CorrelationId}", status, correlationId);
                    response.EnsureSuccessStatusCode();
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType != null && !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("SIS export returned non-JSON content-type={ContentType} correlationId={CorrelationId}", contentType, correlationId);
                    throw new SisAttestationExportClientException("Le SIS a renvoyé une réponse non-JSON.");
                }

                var json = await response.Content.ReadAsStringAsync(cts.Token);

                try
                {
                    return _parser.Parse(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SIS export invalid JSON payload correlationId={CorrelationId}", correlationId);
                    throw new SisAttestationExportClientException("Le SIS a renvoyé un JSON invalide.", ex);
                }
            }
            catch (SisAttestationExportClientException)
            {
                throw;
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                _logger.LogError(ex, "SIS export timeout attempt={Attempt} correlationId={CorrelationId}", attempt, correlationId);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                _logger.LogError(ex, "SIS export HTTP error attempt={Attempt} correlationId={CorrelationId}", attempt, correlationId);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, "SIS export unexpected error attempt={Attempt} correlationId={CorrelationId}", attempt, correlationId);
            }

            if (attempt <= retryCount)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
        }

        throw new SisAttestationExportClientException(
            "Impossible de contacter le SIS pour l'export des attestations.",
            lastException ?? new Exception("Unknown error"));
    }

    private int TryGetInt(string key, int defaultValue)
    {
        var str = _configuration[key];
        return int.TryParse(str, out var parsed) ? parsed : defaultValue;
    }

    private string GetCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue(CorrelationIdItemKey, out var value) == true && value != null)
        {
            return value.ToString() ?? Guid.NewGuid().ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
