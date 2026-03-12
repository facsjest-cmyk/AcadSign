using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

Console.WriteLine("Test d'authentification via API Backend");
Console.WriteLine("=========================================\n");

var httpClient = new HttpClient();
var apiUrl = "http://localhost:5000/api/auth/login";

var loginRequest = new { Username = "superuser", Password = "hkiko1969**TT" };

try
{
    Console.WriteLine($"Envoi de la requête à : {apiUrl}");
    Console.WriteLine($"Username: {loginRequest.Username}");
    Console.WriteLine("Envoi en cours...\n");

    var response = await httpClient.PostAsJsonAsync(apiUrl, loginRequest);
    
    Console.WriteLine($"Status Code: {response.StatusCode}");
    
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"\n✅ Succès !\nRéponse: {content}");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"\n❌ Échec !\nErreur: {error}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Exception: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}

Console.WriteLine("\nAppuyez sur une touche pour quitter...");
Console.ReadKey();
