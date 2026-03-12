using System;
using BCrypt.Net;

var password = "hkiko1969**TT";
var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine();
Console.WriteLine("SQL pour mettre à jour:");
Console.WriteLine($"UPDATE \"AppUsers\" SET \"PasswordHash\" = '{hash}' WHERE \"Username\" = 'superuser';");
