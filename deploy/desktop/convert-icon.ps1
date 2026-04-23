param(
  [Parameter(Mandatory=$true)]
  [string]$SourcePng,

  [Parameter(Mandatory=$true)]
  [string]$OutputIco
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $SourcePng)) {
  throw "Source PNG not found: $SourcePng"
}

$destDir = Split-Path -Parent $OutputIco
if ($destDir -and -not (Test-Path -LiteralPath $destDir)) {
  New-Item -ItemType Directory -Force -Path $destDir | Out-Null
}

if (Test-Path -LiteralPath $OutputIco) {
  $backup = "$OutputIco.bad.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
  Copy-Item -LiteralPath $OutputIco -Destination $backup -Force
}

Add-Type -AssemblyName System.Drawing

function New-PngBytes {
  param(
    [Parameter(Mandatory=$true)]
    [System.Drawing.Image]$Image,

    [Parameter(Mandatory=$true)]
    [int]$Size
  )

  $bmp = $null
  $g = $null
  $ms = $null

  try {
    $bmp = New-Object System.Drawing.Bitmap $Size, $Size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.DrawImage($Image, 0, 0, $Size, $Size)

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    return ,$ms.ToArray()
  }
  finally {
    if ($g) { $g.Dispose() }
    if ($bmp) { $bmp.Dispose() }
    if ($ms) { $ms.Dispose() }
  }
}

$img = $null
$fs = $null
$bw = $null

try {
  $img = [System.Drawing.Image]::FromFile((Resolve-Path -LiteralPath $SourcePng))

  # include multiple sizes for best Windows support
  $sizes = @(256, 48, 32, 16)
  $images = foreach ($s in $sizes) { ,(New-PngBytes -Image $img -Size $s) }

  $fs = New-Object System.IO.FileStream($OutputIco, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
  $bw = New-Object System.IO.BinaryWriter($fs)

  # ICONDIR
  $bw.Write([UInt16]0)  # reserved
  $bw.Write([UInt16]1)  # type
  $bw.Write([UInt16]$images.Count)

  $offset = 6 + (16 * $images.Count)

  # ICONDIRENTRY
  for ($i = 0; $i -lt $images.Count; $i++) {
    $bytes = $images[$i]
    $s = $sizes[$i]

    $w = if ($s -ge 256) { 0 } else { [byte]$s }
    $h = if ($s -ge 256) { 0 } else { [byte]$s }

    $bw.Write([byte]$w)          # width
    $bw.Write([byte]$h)          # height
    $bw.Write([byte]0)           # color count
    $bw.Write([byte]0)           # reserved
    $bw.Write([UInt16]1)         # planes
    $bw.Write([UInt16]32)        # bit count
    $bw.Write([UInt32]$bytes.Length)  # bytes in resource
    $bw.Write([UInt32]$offset)        # image offset

    $offset += $bytes.Length
  }

  # image data
  foreach ($bytes in $images) {
    $bw.Write($bytes)
  }

  $bw.Flush()
}
finally {
  if ($img) { $img.Dispose() }
  if ($bw) { $bw.Dispose() }
  if ($fs) { $fs.Dispose() }
}

Write-Host "ICO generated: $OutputIco"
