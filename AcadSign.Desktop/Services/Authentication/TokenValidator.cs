using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace AcadSign.Desktop.Services.Authentication;

public class TokenValidator
{
    public bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return true;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Vérifier l'expiration avec une marge de 5 minutes
            var expirationTime = jwtToken.ValidTo;
            var now = DateTime.UtcNow;

            return now.AddMinutes(5) >= expirationTime;
        }
        catch (Exception)
        {
            return true; // Token invalide = considéré comme expiré
        }
    }

    public DateTime? GetTokenExpiration(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserEmail(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserName(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserRole(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        }
        catch
        {
            return null;
        }
    }
}
