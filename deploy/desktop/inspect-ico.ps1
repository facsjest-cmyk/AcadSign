param(
  [Parameter(Mandatory=$true)]
  [string]$IcoPath
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $IcoPath)) {
  throw "ICO not found: $IcoPath"
}

$bytes = [System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $IcoPath))
if ($bytes.Length -lt 6) {
  throw "File too small to be an ICO."
}

$reserved = [BitConverter]::ToUInt16($bytes, 0)
$type = [BitConverter]::ToUInt16($bytes, 2)
$count = [BitConverter]::ToUInt16($bytes, 4)

"reserved=$reserved type=$type count=$count sizeBytes=$($bytes.Length)"

for ($i = 0; $i -lt $count; $i++) {
  $off = 6 + (16 * $i)
  if ($off + 16 -gt $bytes.Length) { break }

  $w = $bytes[$off]
  $h = $bytes[$off + 1]
  if ($w -eq 0) { $w = 256 }
  if ($h -eq 0) { $h = 256 }

  $planes = [BitConverter]::ToUInt16($bytes, $off + 4)
  $bpp = [BitConverter]::ToUInt16($bytes, $off + 6)
  $imgSize = [BitConverter]::ToUInt32($bytes, $off + 8)
  $imgOff = [BitConverter]::ToUInt32($bytes, $off + 12)

  "#$i ${w}x${h} planes=$planes bpp=$bpp imgSize=$imgSize imgOffset=$imgOff"
}
