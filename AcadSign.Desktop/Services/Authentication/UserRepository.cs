using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Authentication;

public class UserRepository : IUserRepository
{
    public UserRepository()
    {
    }

    public Task<UserDto?> GetByUsernameAsync(string username)
    {
        // Cette méthode n'est plus utilisée avec l'authentification via API
        // Elle est conservée pour compatibilité avec l'interface
        return Task.FromResult<UserDto?>(null);
    }

    public Task<UserDto?> GetByIdAsync(Guid id)
    {
        // Méthode conservée pour compatibilité
        return Task.FromResult<UserDto?>(null);
    }

    public Task<UserDto> CreateAsync(UserDto user)
    {
        // Méthode conservée pour compatibilité
        return Task.FromResult(user);
    }

    public Task UpdateLastLoginAsync(Guid userId)
    {
        // La mise à jour est gérée par l'API backend
        return Task.CompletedTask;
    }
}
