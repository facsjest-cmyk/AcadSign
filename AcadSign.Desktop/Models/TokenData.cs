using System;

namespace AcadSign.Desktop.Models;

public class TokenData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; }
}
