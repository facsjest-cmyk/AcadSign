using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using AcadSign.Desktop.Models;

namespace AcadSign.Desktop.Services.Storage;

public class TokenStorageService : ITokenStorageService
{
    private static readonly string TokenFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AcadSign",
        "tokens.dat");

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
            var plainBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

            var dir = Path.GetDirectoryName(TokenFilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(TokenFilePath, protectedBytes);
        });
    }

    public Task<(string? accessToken, string? refreshToken)> GetTokensAsync()
    {
        return Task.Run<(string?, string?)>(() =>
        {
            if (!File.Exists(TokenFilePath))
            {
                return (null, null);
            }

            try
            {
                var protectedBytes = File.ReadAllBytes(TokenFilePath);
                var plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                var json = System.Text.Encoding.UTF8.GetString(plainBytes);
                var tokenData = JsonSerializer.Deserialize<TokenData>(json);
                if (tokenData == null)
                {
                    return (null, null);
                }

                return (tokenData.AccessToken, tokenData.RefreshToken);
            }
            catch (Exception)
            {
                return (null, null);
            }
        });
    }

    public Task DeleteTokensAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                if (File.Exists(TokenFilePath))
                {
                    File.Delete(TokenFilePath);
                }
            }
            catch
            {
            }
        });
    }

    public async Task<bool> HasTokensAsync()
    {
        var (accessToken, refreshToken) = await GetTokensAsync();
        return !string.IsNullOrEmpty(accessToken) || !string.IsNullOrEmpty(refreshToken);
    }
}
