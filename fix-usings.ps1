# Script pour ajouter les using statements manquants dans les fichiers C#

$files = Get-ChildItem -Path "c:\e-sign\AcadSign.Desktop" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Skip if file already has proper usings
    if ($content -match "using System;") {
        continue
    }
    
    # Detect what usings are needed
    $needsSystem = $content -match "\b(Guid|DateTime|Array|Exception|EventArgs|IDisposable|TimeSpan|Uri)\b"
    $needsTask = $content -match "\bTask\b"
    $needsCollections = $content -match "\b(List|Dictionary|IEnumerable|Collection)\b"
    $needsLinq = $content -match "\b(Select|Where|FirstOrDefault|ToList|Any)\b"
    $needsLogging = $content -match "\bILogger\b"
    $needsHttp = $content -match "\bHttpClient\b"
    $needsIO = $content -match "\b(File|Directory|Path|Stream|StreamReader|StreamWriter)\b"
    
    # Find the namespace line
    if ($content -match "namespace ([^;]+);") {
        $namespaceMatch = $matches[0]
        $usings = @()
        
        if ($needsSystem) { $usings += "using System;" }
        if ($needsCollections) { $usings += "using System.Collections.Generic;" }
        if ($needsLinq) { $usings += "using System.Linq;" }
        if ($needsTask) { $usings += "using System.Threading.Tasks;" }
        if ($needsHttp) { $usings += "using System.Net.Http;" }
        if ($needsIO) { $usings += "using System.IO;" }
        if ($needsLogging) { $usings += "using Microsoft.Extensions.Logging;" }
        
        if ($usings.Count -gt 0) {
            $usingBlock = ($usings -join "`n") + "`n`n"
            $newContent = $content -replace "namespace ", "$usingBlock`namespace "
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
            Write-Host "Fixed: $($file.Name)"
        }
    }
}

Write-Host "Done!"
