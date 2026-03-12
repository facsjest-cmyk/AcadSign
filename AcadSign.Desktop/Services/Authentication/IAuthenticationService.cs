using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<string> RefreshTokenAsync(string refreshToken);
}

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
