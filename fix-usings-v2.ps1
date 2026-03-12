# Script pour corriger les using statements mal placés

$files = Get-ChildItem -Path "c:\e-sign\AcadSign.Desktop" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Fix files where usings were inserted incorrectly (before namespace instead of at top)
    if ($content -match "using System;\s*using System\.Collections\.Generic;\s*using System\.Linq;\s*using System\.Threading\.Tasks;\s*namespace") {
        # Extract the using statements
        $pattern = "(using [^;]+;\s*)+"
        if ($content -match $pattern) {
            $usings = $matches[0]
            # Remove the usings from their current position
            $content = $content -replace $pattern, ""
            # Add them at the top
            $content = $usings + "`n" + $content
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "Fixed: $($file.Name)"
        }
    }
}

Write-Host "Done!"
