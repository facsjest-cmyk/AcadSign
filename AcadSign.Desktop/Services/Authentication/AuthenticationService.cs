namespace AcadSign.Desktop.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    public async Task<AuthenticationResult> LoginAsync()
    {
        await Task.Delay(1000);
        
        return new AuthenticationResult
        {
            AccessToken = "mock_access_token",
            RefreshToken = "mock_refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
    
    public async Task LogoutAsync()
    {
        await Task.Delay(100);
    }
    
    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        await Task.Delay(500);
        return "new_access_token";
    }
}
