param(
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64",
  [string]$Version = "1.0.0",
  [string]$IsccPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\")).Path
$csproj = Join-Path $repoRoot "AcadSign.Desktop\AcadSign.Desktop.csproj"

$publishDir = Join-Path $PSScriptRoot "publish"
$outputDir = Join-Path $PSScriptRoot "output"

if (Test-Path $publishDir) {
  Remove-Item -Path $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Write-Host "Publishing Desktop app..."
& dotnet publish $csproj -c $Configuration -r $Rid -o $publishDir /p:SelfContained=true /p:PublishReadyToRun=true

$isccCandidates = @()

if ($IsccPath -and (Test-Path $IsccPath)) {
  $isccCandidates += $IsccPath
}

if ($env:ISCC -and (Test-Path $env:ISCC)) {
  $isccCandidates += $env:ISCC
}

$isccCmd = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
if ($isccCmd -and $isccCmd.Source -and (Test-Path $isccCmd.Source)) {
  $isccCandidates += $isccCmd.Source
}

$uninstallRoots = @(
  "HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
  "HKLM:\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
  "HKCU:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
)

foreach ($root in $uninstallRoots) {
  if (-not (Test-Path $root)) {
    continue
  }

  Get-ChildItem -Path $root -ErrorAction SilentlyContinue | ForEach-Object {
    try {
      $props = Get-ItemProperty -Path $_.PSPath -ErrorAction Stop
    } catch {
      return
    }

    if ($props.DisplayName -and ($props.DisplayName -like "*Inno Setup*")) {
      $installLocation = $props.InstallLocation
      if ($installLocation) {
        $candidate = Join-Path $installLocation "ISCC.exe"
        if (Test-Path $candidate) {
          $isccCandidates += $candidate
        }
      }

      if ($props.UninstallString) {
        $uninstallExe = ($props.UninstallString -replace '^\s*"?([^"\s]+).*$', '$1')
        if ($uninstallExe -and (Test-Path $uninstallExe)) {
          $folder = Split-Path -Path $uninstallExe -Parent
          $candidate = Join-Path $folder "ISCC.exe"
          if (Test-Path $candidate) {
            $isccCandidates += $candidate
          }
        }
      }
    }
  }
}

$isccCandidates += @(
  (Join-Path $env:LOCALAPPDATA "Programs\\Inno Setup 6\\ISCC.exe"),
  (Join-Path $env:LOCALAPPDATA "Programs\\Inno Setup\\ISCC.exe"),
  "C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe",
  "C:\\Program Files\\Inno Setup 6\\ISCC.exe"
)

$isccCandidates = $isccCandidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

if (-not $isccCandidates -or $isccCandidates.Count -eq 0) {
  throw "Inno Setup Compiler (ISCC.exe) not found. Install Inno Setup 6 and/or pass -IsccPath 'C:\\Path\\To\\ISCC.exe' or set ISCC environment variable."
}

$iscc = $isccCandidates[0]
$iss = Join-Path $PSScriptRoot "installer.iss"

Write-Host "Building installer..."
& $iscc $iss "/DMyAppVersion=$Version"

Write-Host "Done. Installer generated under: $outputDir"
