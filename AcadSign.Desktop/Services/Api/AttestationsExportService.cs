using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AcadSign.Desktop.Models;
using AcadSign.Desktop.Services.Storage;

namespace AcadSign.Desktop.Services.Api;

public interface IAttestationsExportService
{
    Task<IReadOnlyList<DownloadedAttestationPdf>> DownloadDayAsync(DateTime utcDay, string outputRoot);
}

public class AttestationsExportService : IAttestationsExportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStorageService _tokenStorage;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AttestationsExportService(IHttpClientFactory httpClientFactory, ITokenStorageService tokenStorage)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
        _baseUrl = "http://10.2.22.210".TrimEnd('/');
    }

    public async Task<IReadOnlyList<DownloadedAttestationPdf>> DownloadDayAsync(DateTime utcDay, string outputRoot)
    {
        var client = _httpClientFactory.CreateClient();
        
        var tokens = await _tokenStorage.GetTokensAsync();
        if (!string.IsNullOrEmpty(tokens.accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.accessToken);
        }

        Directory.CreateDirectory(outputRoot);

        var results = new List<DownloadedAttestationPdf>();
        var page = 1;
        const int perPage = 200;

        while (true)
        {
            var url = $"{_baseUrl}/api/v1/admin/attestations/export.json?page={page}&perPage={perPage}";

            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var rawJson = await response.Content.ReadAsStringAsync();
            try { System.IO.File.WriteAllText($@"C:\Users\viet\Desktop\acadsign\api-raw-page{page}.json", rawJson); } catch {}

            var payload = JsonSerializer.Deserialize<AttestationsExportResponse>(rawJson, JsonOptions)
                          ?? new AttestationsExportResponse();

            if (payload.Data.Count == 0)
            {
                break;
            }

            foreach (var item in payload.Data)
            {
                if (string.IsNullOrWhiteSpace(item.PdfUrl))
                    continue;

                var pdfUri = item.PdfUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? item.PdfUrl
                    : $"{_baseUrl}{item.PdfUrl}";

                using var pdfResponse = await client.GetAsync(pdfUri);
                pdfResponse.EnsureSuccessStatusCode();

                var safeFileName = string.IsNullOrWhiteSpace(item.FileName)
                    ? $"attestation-{item.Id}.pdf"
                    : item.FileName;

                var localPath = Path.Combine(outputRoot, safeFileName);

                await using (var fs = File.Create(localPath))
                {
                    await pdfResponse.Content.CopyToAsync(fs);
                }

                results.Add(new DownloadedAttestationPdf
                {
                    Id = item.Id,
                    FileName = safeFileName,
                    LocalPath = localPath,
                    ProcessedAt = item.ProcessedAt
                });
            }

            if (payload.Meta == null || payload.Meta.TotalPages <= page)
            {
                break;
            }

            page++;
        }

        return results;
    }
}
