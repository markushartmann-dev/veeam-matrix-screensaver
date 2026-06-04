# Install-VeeaMatrix.ps1
# VeeaMatrix Screensaver — Installer
#
# Usage: right-click -> "Mit PowerShell ausführen" / "Run with PowerShell"
#        Place this script in the same folder as VeeaMatrix.scr

# ── Self-elevate to Administrator if needed ───────────────────────────────────
if (-not ([Security.Principal.WindowsPrincipal]
          [Security.Principal.WindowsIdentity]::GetCurrent()
         ).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator))
{
    Start-Process powershell.exe `
        "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" `
        -Verb RunAs
    exit
}

$host.UI.RawUI.WindowTitle = "VeeaMatrix Installer"
Write-Host ""
Write-Host "  ██╗   ██╗███████╗███████╗ █████╗ ███╗   ███╗ █████╗ ████████╗██████╗ ██╗██╗  ██╗" -ForegroundColor Green
Write-Host "  ██║   ██║██╔════╝██╔════╝██╔══██╗████╗ ████║██╔══██╗╚══██╔══╝██╔══██╗██║╚██╗██╔╝" -ForegroundColor Green
Write-Host "  ██║   ██║█████╗  █████╗  ███████║██╔████╔██║███████║   ██║   ██████╔╝██║ ╚███╔╝ " -ForegroundColor Green
Write-Host "  ╚██╗ ██╔╝██╔══╝  ██╔══╝  ██╔══██║██║╚██╔╝██║██╔══██║   ██║   ██╔══██╗██║ ██╔██╗ " -ForegroundColor Green
Write-Host "   ╚████╔╝ ███████╗███████╗██║  ██║██║ ╚═╝ ██║██║  ██║   ██║   ██║  ██║██║██╔╝ ██╗" -ForegroundColor Green
Write-Host "    ╚═══╝  ╚══════╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═╝" -ForegroundColor Green
Write-Host ""
Write-Host "  Screensaver Installer  |  by Markus Hartmann  |  markushartmann.blog" -ForegroundColor DarkGreen
Write-Host ""

# ── Locate VeeaMatrix.scr ─────────────────────────────────────────────────────
$ScriptDir = Split-Path $MyInvocation.MyCommand.Path -ErrorAction SilentlyContinue
if (-not $ScriptDir) { $ScriptDir = $PSScriptRoot }

$ScrSource = Join-Path $ScriptDir "VeeaMatrix.scr"
$ScrDest   = "$env:SystemRoot\System32\VeeaMatrix.scr"

if (-not (Test-Path $ScrSource)) {
    Write-Host "  ERROR: VeeaMatrix.scr not found next to this script." -ForegroundColor Red
    Write-Host "  Please place Install-VeeaMatrix.ps1 and VeeaMatrix.scr in the same folder." -ForegroundColor Yellow
    Write-Host ""
    Read-Host "  Press Enter to exit"
    exit 1
}

Write-Host "  Source : $ScrSource" -ForegroundColor Gray
Write-Host "  Target : $ScrDest" -ForegroundColor Gray
Write-Host ""

# ── Unblock source (remove Zone.Identifier / Mark of the Web) ────────────────
try {
    Unblock-File -Path $ScrSource -ErrorAction SilentlyContinue
    Write-Host "  [OK] Zone.Identifier removed from source file" -ForegroundColor Green
} catch {}

# ── Copy to System32 ──────────────────────────────────────────────────────────
Write-Host "  Copying to System32..." -ForegroundColor Cyan
try {
    Copy-Item -Path $ScrSource -Destination $ScrDest -Force
    Write-Host "  [OK] VeeaMatrix.scr installed to System32" -ForegroundColor Green
} catch {
    Write-Host ""
    Write-Host "  ERROR: Could not copy to System32." -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Write-Host ""
    Read-Host "  Press Enter to exit"
    exit 1
}

# ── Set as active screensaver? ────────────────────────────────────────────────
Write-Host ""
$choice = Read-Host "  Set VeeaMatrix as your active screensaver now? (J/Y = Yes, N = No)"
if ($choice -match "^[YyJj]") {
    $regPath = "HKCU:\Control Panel\Desktop"
    Set-ItemProperty -Path $regPath -Name "SCRNSAVE.EXE"      -Value $ScrDest -Type String
    Set-ItemProperty -Path $regPath -Name "ScreenSaveActive"   -Value "1"     -Type String
    Set-ItemProperty -Path $regPath -Name "ScreenSaveTimeOut"  -Value "300"   -Type String  # 5 min
    Write-Host "  [OK] VeeaMatrix set as active screensaver (5 min timeout)" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Tip: Right-click Desktop -> Personalize -> Lock screen -> Screen saver" -ForegroundColor DarkCyan
    Write-Host "       to change the timeout — or right-click VeeaMatrix.scr -> Configure" -ForegroundColor DarkCyan
}

# ── Done ──────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor Green
Write-Host "  Installation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "  Right-click VeeaMatrix.scr -> Konfigurieren / Configure" -ForegroundColor Cyan
Write-Host "  to open the settings dialog." -ForegroundColor Cyan
Write-Host "  ============================================================" -ForegroundColor Green
Write-Host ""
Read-Host "  Press Enter to exit"
