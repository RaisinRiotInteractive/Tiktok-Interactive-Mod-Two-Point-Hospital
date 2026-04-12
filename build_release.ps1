# build_release.ps1 — builds all release artefacts and optionally creates a GitHub release
#
# Usage:
#   .\build_release.ps1                          # build only
#   .\build_release.ps1 -CreateRelease           # build + push GitHub release (requires gh CLI)
#   .\build_release.ps1 -Version "1.2.0"         # override version string

param(
    [string]$Version      = "1.0.0",
    [switch]$CreateRelease
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot

function Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function OK($msg)   { Write-Host "    $msg" -ForegroundColor Green }
function Fail($msg) { Write-Host "    ERROR: $msg" -ForegroundColor Red; exit 1 }

Set-Location $Root

# ── 1. Build mod DLL ─────────────────────────────────────────────────
Step "Building mod (TPH_TikTokMod.dll)..."
dotnet build TPH_TikTokMod.csproj -c Release --nologo -v q
$ModDll = Join-Path $Root "bin\Release\net472\TPH_TikTokMod.dll"
if (-not (Test-Path $ModDll)) { Fail "Mod DLL not found at $ModDll" }
OK "Built: $ModDll"

# ── 2. Publish companion app ──────────────────────────────────────────
Step "Publishing companion app (TPH_TikTokCompanion.exe)..."
$CompanionOut = Join-Path $Root "installer\TPH_TikTokInstaller\Resources\companion_tmp"
dotnet publish companion\TPH_TikTokCompanion\TPH_TikTokCompanion.csproj `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $CompanionOut --nologo -v q

$CompanionExe = Join-Path $CompanionOut "TPH_TikTokCompanion.exe"
if (-not (Test-Path $CompanionExe)) { Fail "Companion EXE not found at $CompanionExe" }
OK "Published: $CompanionExe"

# ── 3. Stage resources for installer ─────────────────────────────────
Step "Staging installer resources..."
$ResDir = Join-Path $Root "installer\TPH_TikTokInstaller\Resources"
Copy-Item $ModDll    (Join-Path $ResDir "TPH_TikTokMod.dll")        -Force
Copy-Item $CompanionExe (Join-Path $ResDir "TPH_TikTokCompanion.exe") -Force
Remove-Item $CompanionOut -Recurse -Force -ErrorAction SilentlyContinue
OK "Resources ready"

# ── 4. Build installer ────────────────────────────────────────────────
Step "Building installer..."
$InstallerOut = Join-Path $Root "release\installer_tmp"
dotnet publish installer\TPH_TikTokInstaller\TPH_TikTokInstaller.csproj `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $InstallerOut --nologo -v q

$InstallerExe = Join-Path $InstallerOut "TPH_TikTokInstaller.exe"
if (-not (Test-Path $InstallerExe)) { Fail "Installer EXE not found" }
OK "Built installer"

# ── 5. Create release folder ──────────────────────────────────────────
Step "Preparing release artefacts..."
$ReleaseDir = Join-Path $Root "release"
New-Item -ItemType Directory -Force -Path $ReleaseDir | Out-Null

$InstallerFinal = Join-Path $ReleaseDir "TPH_TikTokMod_v${Version}_Installer.exe"
Copy-Item $InstallerExe $InstallerFinal -Force
Remove-Item $InstallerOut -Recurse -Force -ErrorAction SilentlyContinue
OK "Installer: $InstallerFinal"

# ── 6. Create manual install zip ──────────────────────────────────────
Step "Creating manual install zip..."
$TmpDir  = Join-Path $ReleaseDir "zip_tmp\TPH_TikTokMod_v$Version"
New-Item -ItemType Directory -Force -Path "$TmpDir\BepInEx\plugins"     | Out-Null
New-Item -ItemType Directory -Force -Path "$TmpDir\TPH_TikTokCompanion" | Out-Null

Copy-Item $ModDll (Join-Path $TmpDir "BepInEx\plugins\TPH_TikTokMod.dll")
Copy-Item (Join-Path $ResDir "TPH_TikTokCompanion.exe") `
          (Join-Path $TmpDir "TPH_TikTokCompanion\TPH_TikTokCompanion.exe")

@"
# TikTok Live Mod for Two Point Hospital — Manual Installation
# v$Version

Requirements
------------
• Two Point Hospital (Steam)
• BepInEx 5.x (x64) — https://github.com/BepInEx/BepInEx/releases

Steps
-----
1. Install BepInEx into your TPH folder if not already installed.

2. Copy  BepInEx\plugins\TPH_TikTokMod.dll
   into  [TPH folder]\BepInEx\plugins\

3. Copy the  TPH_TikTokCompanion\  folder
   into  [TPH folder]\

4. Launch Two Point Hospital, then run
       [TPH folder]\TPH_TikTokCompanion\TPH_TikTokCompanion.exe

5. Enter your TikTok username on the Live tab and click Connect.
   The in-game overlay (top-right) will show green when active.

Tips
----
• Press F9 in-game to hide/show the overlay.
• Configure event rules on the Rules tab of the companion app.
• Click Save Rules after making changes.

Source & support
----------------
https://github.com/RaisinRiotInteractive/Tiktok-Interactive-Mod-Two-Point-Hospital
"@ | Set-Content (Join-Path $TmpDir "INSTALL.txt") -Encoding UTF8

$ZipPath = Join-Path $ReleaseDir "TPH_TikTokMod_v${Version}_Manual.zip"
Compress-Archive -Path (Join-Path $ReleaseDir "zip_tmp\*") -DestinationPath $ZipPath -Force
Remove-Item (Join-Path $ReleaseDir "zip_tmp") -Recurse -Force
OK "Zip: $ZipPath"

# ── 7. Optional GitHub release ────────────────────────────────────────
if ($CreateRelease)
{
    Step "Creating GitHub release v$Version..."

    $Notes = @"
## TikTok Live Mod for Two Point Hospital — v$Version

### Install options
| | |
|---|---|
| **Installer** (recommended) | Download `TPH_TikTokMod_v${Version}_Installer.exe` and run it. It auto-detects your game, installs BepInEx if needed, and copies all files. |
| **Manual zip** | Download `TPH_TikTokMod_v${Version}_Manual.zip`, extract, and follow `INSTALL.txt`. |

### Requirements
- Two Point Hospital (Steam)
- Windows 10 / 11

### What's included
- `TPH_TikTokMod.dll` — BepInEx mod (auto-installed to `BepInEx/plugins/`)
- `TPH_TikTokCompanion.exe` — companion app (auto-installed to `TPH_TikTokCompanion/`)

### Getting started
1. Run the installer (or follow the manual steps in the zip)
2. Launch Two Point Hospital
3. Run `TPH_TikTokCompanion.exe` from your TPH folder
4. Enter your TikTok username and click **Connect**

The in-game overlay (top-right corner) shows the connection status. Press **F9** to hide/show it.
"@

    gh release create "v$Version" `
        $InstallerFinal `
        $ZipPath `
        --title "v$Version" `
        --notes $Notes

    OK "GitHub release v$Version created."
}

Step "Done!"
Write-Host ""
Write-Host "  Installer : release\TPH_TikTokMod_v${Version}_Installer.exe" -ForegroundColor White
Write-Host "  Manual zip: release\TPH_TikTokMod_v${Version}_Manual.zip"    -ForegroundColor White
Write-Host ""
