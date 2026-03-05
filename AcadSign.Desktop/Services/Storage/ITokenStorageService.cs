using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Storage;

public interface ITokenStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken);
    Task<(string? accessToken, string? refreshToken)> GetTokensAsync();
    Task DeleteTokensAsync();
    Task<bool> HasTokensAsync();
}
