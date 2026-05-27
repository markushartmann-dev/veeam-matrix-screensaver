# Build-VeeamMatrix.ps1
# Kompiliert VeeamMatrix.cs -> VeeamMatrix.scr
# Danach: VeeamMatrix.scr in C:\Windows\System32 kopieren (als Admin),
#         oder per Rechtsklick > "Installieren"

$src    = Join-Path $PSScriptRoot "VeeamMatrix.cs"
$out    = Join-Path $PSScriptRoot "VeeamMatrix.scr"
$csc    = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$refs   = "System.Windows.Forms.dll,System.Drawing.dll,System.dll,Microsoft.VisualBasic.dll"

if (-not (Test-Path $csc)) {
    Write-Host "FEHLER: csc.exe nicht gefunden unter $csc" -ForegroundColor Red
    exit 1
}

Write-Host "Kompiliere $src ..." -ForegroundColor Cyan

$result = & $csc `
    /target:winexe `
    /out:"$out" `
    /r:$refs `
    /optimize+ `
    /nologo `
    "$src" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK  ->  $out" -ForegroundColor Green
    Write-Host ""
    Write-Host "Naechste Schritte:" -ForegroundColor Yellow
    Write-Host "  1) Doppelklick auf VeeamMatrix.scr  -> laeuft sofort"
    Write-Host "  2) Rechtsklick auf VeeamMatrix.scr  -> 'Installieren' (systemweit)"
    Write-Host "     oder manuell nach C:\Windows\System32 kopieren (als Admin)"
    Write-Host "  3) Rechtsklick -> 'Konfigurieren'   -> Einstellungs-Dialog"
    Write-Host "  4) Rechtsklick -> 'Testen'          -> Vollbild-Vorschau"
} else {
    Write-Host "FEHLER beim Kompilieren:" -ForegroundColor Red
    $result | Where-Object { $_ -match "error" } | ForEach-Object { Write-Host $_ -ForegroundColor Red }
}
