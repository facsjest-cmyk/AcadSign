using AcadSign.Backend.Application.Auth.Commands;

namespace AcadSign.Backend.Application.Auth.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string username, string password);
}
