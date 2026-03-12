using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcadSign.Backend.Application.Auth.Commands;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AcadSign.Backend.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            // Récupérer l'utilisateur
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect."
                };
            }

            // Vérifier le mot de passe avec BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect."
                };
            }

            // Mettre à jour la date de dernière connexion
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);

            // Générer le JWT token
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new LoginResponse
            {
                Success = true,
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = $"Erreur lors de la connexion : {ex.Message}"
            };
        }
    }

    private string GenerateJwtToken(AppUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "AcadSign-Super-Secret-Key-2026-MinLength32Characters!"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var role = user.Role switch
        {
            UserRole.Admin => Roles.Administrator,
            UserRole.SuperUser => Roles.Administrator,
            UserRole.ApiClient => Roles.ApiClient,
            _ => Roles.Registrar
        };

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "AcadSign.Backend",
            audience: _configuration["Jwt:Audience"] ?? "AcadSign.Desktop",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
