namespace AcadSign.Desktop.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync();
    Task LogoutAsync();
    Task<string> RefreshTokenAsync(string refreshToken);
}

public class AuthenticationResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
