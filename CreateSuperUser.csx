#!/usr/bin/env dotnet-script
#r "nuget: BCrypt.Net-Next, 4.1.0"
#r "nuget: Npgsql, 8.0.0"

using BCrypt.Net;
using Npgsql;
using System;

// Générer le hash du mot de passe
var password = "hkiko1969**TT";
var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 12);

Console.WriteLine($"Hash généré: {passwordHash}");

// Connexion à PostgreSQL
var connectionString = "Host=localhost;Port=5432;Database=acadsign;Username=acadsign_user;Password=AcadSign2026Dev!";

using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

// Insérer l'utilisateur superuser
var sql = @"
INSERT INTO ""AppUsers"" (""Id"", ""Username"", ""Email"", ""PasswordHash"", ""Role"", ""IsActive"", ""CreatedAt"")
VALUES (@id, @username, @email, @passwordHash, @role, @isActive, @createdAt)
ON CONFLICT (""Username"") DO UPDATE 
SET ""PasswordHash"" = @passwordHash, ""Role"" = @role, ""IsActive"" = @isActive
RETURNING ""Id"", ""Username"", ""Email"", ""Role"";
";

using var cmd = new NpgsqlCommand(sql, conn);
cmd.Parameters.AddWithValue("id", Guid.NewGuid());
cmd.Parameters.AddWithValue("username", "superuser");
cmd.Parameters.AddWithValue("email", "superuser@acadsign.com");
cmd.Parameters.AddWithValue("passwordHash", passwordHash);
cmd.Parameters.AddWithValue("role", 2); // SuperUser
cmd.Parameters.AddWithValue("isActive", true);
cmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);

using var reader = await cmd.ExecuteReaderAsync();
if (await reader.ReadAsync())
{
    Console.WriteLine("✅ Utilisateur superuser créé/mis à jour avec succès !");
    Console.WriteLine($"   ID: {reader.GetGuid(0)}");
    Console.WriteLine($"   Username: {reader.GetString(1)}");
    Console.WriteLine($"   Email: {reader.GetString(2)}");
    Console.WriteLine($"   Role: {reader.GetInt32(3)} (SuperUser)");
    Console.WriteLine($"\n🔑 Identifiants de connexion:");
    Console.WriteLine($"   Username: superuser");
    Console.WriteLine($"   Password: hkiko1969**TT");
}
