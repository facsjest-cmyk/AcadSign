using System;
using System.Threading.Tasks;
using AcadSign.Desktop.Services.Authentication;

namespace AcadSign.Desktop.Scripts;

public class CreateSuperUser
{
    public static async Task Main(string[] args)
    {
        var passwordHasher = new PasswordHasher();
        var userRepository = new UserRepository();

        // Créer l'utilisateur superuser
        var superUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Username = "superuser",
            Email = "superuser@acadsign.com",
            PasswordHash = passwordHasher.HashPassword("hkiko1969**TT"),
            Role = UserRoleDto.SuperUser,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await userRepository.CreateAsync(superUser);
            Console.WriteLine("✅ Utilisateur superuser créé avec succès !");
            Console.WriteLine($"   Username: {superUser.Username}");
            Console.WriteLine($"   Email: {superUser.Email}");
            Console.WriteLine($"   Role: {superUser.Role}");
            Console.WriteLine($"   ID: {superUser.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de la création de l'utilisateur : {ex.Message}");
        }
    }
}
