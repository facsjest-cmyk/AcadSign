using System;
using System.Text.Json;
using System.Threading.Tasks;
using AcadSign.Desktop.Models;
using CredentialManagement;

namespace AcadSign.Desktop.Services.Storage;

public class TokenStorageService : ITokenStorageService
{
    private const string CredentialTarget = "AcadSign.Desktop.Tokens";

    public Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        return Task.Run(() =>
        {
            var tokenData = new TokenData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                SavedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(tokenData);

            using var credential = new Credential
            {
                Target = CredentialTarget,
                Username = "AcadSignUser",
                Password = json,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            };

            if (!credential.Save())
            {
                throw new InvalidOperationException("Failed to save tokens to Windows Credential Manager");
            }
        });
    }

    public Task<(string? accessToken, string? refreshToken)> GetTokensAsync()
    {
        return Task.Run<(string?, string?)>(() =>
        {
            using var credential = new Credential
            {
                Target = CredentialTarget
            };

            if (!credential.Load())
            {
                return (null, null);
            }

            try
            {
                var tokenData = JsonSerializer.Deserialize<TokenData>(credential.Password);
                if (tokenData == null)
                {
                    return (null, null);
                }

                return (tokenData.AccessToken, tokenData.RefreshToken);
            }
            catch (JsonException)
            {
                return (null, null);
            }
        });
    }

    public Task DeleteTokensAsync()
    {
        return Task.Run(() =>
        {
            using var credential = new Credential
            {
                Target = CredentialTarget
            };

            credential.Delete();
        });
    }

    public async Task<bool> HasTokensAsync()
    {
        var (accessToken, refreshToken) = await GetTokensAsync();
        return !string.IsNullOrEmpty(accessToken) || !string.IsNullOrEmpty(refreshToken);
    }
}
