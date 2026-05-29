# Build-VeeaMatrix.ps1
# Compiles VeeaMatrix.cs -> VeeaMatrix.scr
# Then: copy VeeaMatrix.scr to C:\Windows\System32 (as Admin),
#       or right-click > "Install"

$src    = Join-Path $PSScriptRoot "VeeaMatrix.cs"
$out    = Join-Path $PSScriptRoot "VeeaMatrix.scr"
$csc    = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$refs   = "System.Windows.Forms.dll,System.Drawing.dll,System.dll,Microsoft.VisualBasic.dll"

if (-not (Test-Path $csc)) {
    Write-Host "FEHLER: csc.exe nicht gefunden unter $csc" -ForegroundColor Red
    exit 1
}

# Optionally embed a banner image as a compiled resource
$bannerNames = @("VeeaMatrix-banner.jpg","VeeaMatrix-banner.png","VeeaMatrix-banner.jpeg","banner.jpg")
$bannerFile  = $null
foreach ($name in $bannerNames) {
    $p = Join-Path $PSScriptRoot $name
    if (Test-Path $p) { $bannerFile = $p; break }
}

Write-Host "Compiling $src ..." -ForegroundColor Cyan
if ($bannerFile) {
    Write-Host "  Embedding banner: $(Split-Path $bannerFile -Leaf)" -ForegroundColor Cyan
}

$compileArgs = @(
    "/target:winexe",
    "/out:`"$out`"",
    "/r:$refs",
    "/optimize+",
    "/nologo"
)
if ($bannerFile) {
    $compileArgs += "/resource:`"$bannerFile`",VeeaMatrix.banner"
}
$compileArgs += "`"$src`""

$result = & $csc @compileArgs 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK  ->  $out" -ForegroundColor Green
    if ($bannerFile) {
        Write-Host "    Banner embedded: $(Split-Path $bannerFile -Leaf)" -ForegroundColor Green
    }
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1) Double-click VeeaMatrix.scr  -> runs immediately"
    Write-Host "  2) Right-click  VeeaMatrix.scr  -> 'Install' (system-wide)"
    Write-Host "     or copy manually to C:\Windows\System32 (as Admin)"
    Write-Host "  3) Right-click -> 'Configure'   -> Settings dialog"
    Write-Host "  4) Right-click -> 'Test'         -> Fullscreen preview"
} else {
    Write-Host "ERROR compiling:" -ForegroundColor Red
    $result | Where-Object { $_ -match "error" } | ForEach-Object { Write-Host $_ -ForegroundColor Red }
}
