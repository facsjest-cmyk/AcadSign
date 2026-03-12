using System;
using AcadSign.Desktop.Services.Authentication;

var hasher = new PasswordHasher();
var password = "hkiko1969**TT";
var hash = hasher.HashPassword(password);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
