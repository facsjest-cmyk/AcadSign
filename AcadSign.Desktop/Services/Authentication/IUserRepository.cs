using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Authentication;

public interface IUserRepository
{
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(UserDto user);
    Task UpdateLastLoginAsync(Guid userId);
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRoleDto Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public enum UserRoleDto
{
    User = 0,
    Admin = 1,
    SuperUser = 2
}
