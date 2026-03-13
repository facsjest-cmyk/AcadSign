using AcadSign.Backend.Infrastructure.Data;
using Hangfire;
using Hangfire.PostgreSql;
using AcadSign.Backend.Web.Infrastructure;
using AcadSign.Backend.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

// Epic 5 - Hangfire
var connectionString = builder.Configuration.GetConnectionString("AcadSign.BackendDb");
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
{
    Attempts = 6,
    DelaysInSeconds = new[] { 0, 60, 300, 900, 3600, 21600 },
    OnAttemptsExceeded = AttemptsExceededAction.Delete
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = new[] { "critical", "default", "batch" };
    options.ServerName = "AcadSign-Worker";
});

// Accept JWT tokens issued by /api/v1/auth/login
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]
                  ?? "AcadSign-Super-Secret-Key-2026-MinLength32Characters!";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
await app.InitialiseDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
// Désactiver la redirection HTTPS en développement pour permettre HTTP
// app.UseHttpsRedirection();

app.UseCorrelationId();
app.UseStaticFiles();

app.UseRateLimiter();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "AcadSign Background Jobs"
});

app.MapGet("/documents", () => Results.Redirect("/verify.html"));
app.MapGet("/documents/{documentId:guid}", (Guid documentId) =>
    Results.Redirect($"/verify.html?id={documentId}"));

app.MapOpenApi("/api/v1/swagger/{documentName}.json");
app.MapGet("/api/v1/swagger.json", () => Results.Redirect("/api/v1/swagger/v1.json"));
app.MapGet("/api/v1/docs", () => Results.Redirect("/docs/index.html"));

app.MapControllers();

app.Map("/", () => Results.Ok(new { status = "ok" }));

app.MapEndpoints();

app.Run();

public partial class Program { }
