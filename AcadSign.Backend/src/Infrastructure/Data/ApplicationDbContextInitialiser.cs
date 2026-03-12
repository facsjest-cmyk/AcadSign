using AcadSign.Backend.Domain.Constants;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace AcadSign.Backend.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IServiceProvider _serviceProvider;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _serviceProvider = serviceProvider;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var roles = new[]
        {
            new IdentityRole(Roles.Administrator),
            new IdentityRole(Roles.Registrar),
            new IdentityRole(Roles.Auditor),
            new IdentityRole(Roles.ApiClient)
        };

        foreach (var role in roles)
        {
            if (_roleManager.Roles.All(r => r.Name != role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }

        var administratorRole = roles.First(r => r.Name == Roles.Administrator);

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        // Seed OpenIddict scopes and clients
        if (_serviceProvider.GetService<IOpenIddictScopeManager>() != null)
        {
            await OpenIddictSeeder.SeedAsync(_serviceProvider);
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Todo List",
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }

        var seedAdmin = await _context.AppUsers.FirstOrDefaultAsync(u => u.Username == "admin");
        if (seedAdmin == null)
        {
            _context.AppUsers.Add(new AppUser
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@acadsign.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            seedAdmin.IsActive = true;
            seedAdmin.Role = UserRole.Admin;
            if (string.IsNullOrWhiteSpace(seedAdmin.Email))
            {
                seedAdmin.Email = "admin@acadsign.local";
            }
        }

        var seedApiClient = await _context.AppUsers.FirstOrDefaultAsync(u => u.Username == "api-client");
        if (seedApiClient == null)
        {
            _context.AppUsers.Add(new AppUser
            {
                Id = Guid.NewGuid(),
                Username = "api-client",
                Email = "api-client@acadsign.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ApiClient123!"),
                Role = UserRole.ApiClient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            seedApiClient.IsActive = true;
            seedApiClient.Role = UserRole.ApiClient;
            if (string.IsNullOrWhiteSpace(seedApiClient.Email))
            {
                seedApiClient.Email = "api-client@acadsign.local";
            }
        }

        var devStudentId = Guid.Parse("d3b3c1a2-7c68-4c1f-9f25-0d2e4d2c5f3a");
        var devStudent = await _context.Students.FirstOrDefaultAsync(s => s.PublicId == devStudentId);
        if (devStudent == null)
        {
            _context.Students.Add(new Student
            {
                PublicId = devStudentId,
                CIN = "DEV-CIN",
                CNE = "DEV-CNE",
                Email = "student@example.com",
                PhoneNumber = null,
                FirstName = "Dev",
                LastName = "Student",
                DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                InstitutionId = Guid.Empty
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(devStudent.Email))
            {
                devStudent.Email = "student@example.com";
            }
        }

        await _context.SaveChangesAsync();
    }
}
