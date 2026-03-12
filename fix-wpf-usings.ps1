# Script pour ajouter les using statements manquants dans les fichiers C# WPF

$files = Get-ChildItem -Path "c:\e-sign\AcadSign.Desktop" -Filter "*.cs" -Recurse -Exclude "*AssemblyInfo.cs","*Designer.cs"

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $modified = $false
    $newLines = @()
    $usingsToAdd = @()
    $hasNamespace = $false
    $firstNamespaceLine = -1
    
    # Analyze file content
    $content = $lines -join "`n"
    
    # Detect what usings are needed
    if ($content -match '\b(Guid|DateTime|Array|Exception|EventArgs|IDisposable|TimeSpan|Uri|Console)\b' -and $content -notmatch 'using System;') {
        $usingsToAdd += "using System;"
    }
    if ($content -match '\b(List|Dictionary|IEnumerable|Collection|IList)\b' -and $content -notmatch 'using System.Collections.Generic;') {
        $usingsToAdd += "using System.Collections.Generic;"
    }
    if ($content -match '\b(Select|Where|FirstOrDefault|ToList|Any|OrderBy)\b' -and $content -notmatch 'using System.Linq;') {
        $usingsToAdd += "using System.Linq;"
    }
    if ($content -match '\bTask\b' -and $content -notmatch 'using System.Threading.Tasks;') {
        $usingsToAdd += "using System.Threading.Tasks;"
    }
    if ($content -match '\bHttpClient\b' -and $content -notmatch 'using System.Net.Http;') {
        $usingsToAdd += "using System.Net.Http;"
    }
    if ($content -match '\b(File|Directory|Path|Stream|StreamReader|StreamWriter)\b' -and $content -notmatch 'using System.IO;') {
        $usingsToAdd += "using System.IO;"
    }
    if ($content -match '\bILogger\b' -and $content -notmatch 'using Microsoft.Extensions.Logging;') {
        $usingsToAdd += "using Microsoft.Extensions.Logging;"
    }
    
    # Find where to insert usings
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^namespace ') {
            $firstNamespaceLine = $i
            break
        }
    }
    
    if ($usingsToAdd.Count -gt 0 -and $firstNamespaceLine -ge 0) {
        # Find existing usings
        $lastUsingLine = -1
        for ($i = 0; $i -lt $firstNamespaceLine; $i++) {
            if ($lines[$i] -match '^using ') {
                $lastUsingLine = $i
            }
        }
        
        # Insert new usings
        if ($lastUsingLine -ge 0) {
            # Add after last existing using
            $newLines = $lines[0..$lastUsingLine]
            foreach ($using in $usingsToAdd) {
                $newLines += $using
            }
            if ($lastUsingLine + 1 -lt $lines.Count) {
                $newLines += $lines[($lastUsingLine + 1)..($lines.Count - 1)]
            }
        } else {
            # Add before namespace
            if ($firstNamespaceLine -gt 0) {
                $newLines = $lines[0..($firstNamespaceLine - 1)]
            }
            foreach ($using in $usingsToAdd) {
                $newLines += $using
            }
            $newLines += ""
            $newLines += $lines[$firstNamespaceLine..($lines.Count - 1)]
        }
        
        Set-Content -Path $file.FullName -Value $newLines
        Write-Host "Fixed: $($file.Name) - Added $($usingsToAdd.Count) usings"
    }
}

Write-Host "`nDone! Fixed using statements."
