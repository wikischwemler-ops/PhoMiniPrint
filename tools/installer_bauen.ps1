# Baut den Installer artefakte\PhoMiniPrint-Setup-<Version>.exe (F-12, NFR-006).
# 1. Version aus Directory.Build.props lesen (einzige Quelle, FR-018)
# 2. dotnet publish: self-contained für win-x64, .NET-Laufzeit eingebaut
# 3. Inno Setup (ISCC) mit Version und Quellordner aufrufen
# Aufruf: powershell -ExecutionPolicy Bypass -File tools\installer_bauen.ps1

$ErrorActionPreference = 'Stop'
$wurzel = Split-Path -Parent $PSScriptRoot

[xml]$props = Get-Content (Join-Path $wurzel 'Directory.Build.props') -Encoding UTF8
$version = ($props.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Version '$version' entspricht nicht MAJOR.MINOR.PATCH." }

$publish = Join-Path $wurzel 'artefakte\publish'
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }

Write-Host "Veröffentliche PhoMiniPrint $version (self-contained, win-x64) ..."
dotnet publish (Join-Path $wurzel 'src\PhoMiniPrint.App\PhoMiniPrint.App.csproj') `
    -c Release -r win-x64 --self-contained true `
    -p:PublishReadyToRun=true -p:DebugType=none `
    -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish ist fehlgeschlagen.' }

$iscc = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 (ISCC.exe) nicht gefunden.' }

Write-Host "Baue Installer mit $iscc ..."
& $iscc "/DVersion=$version" "/DQuelle=$publish" (Join-Path $wurzel 'installer\PhoMiniPrint.iss')
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup ist fehlgeschlagen.' }

$setup = Join-Path $wurzel "artefakte\PhoMiniPrint-Setup-$version.exe"
Write-Host "Fertig: $setup ($([math]::Round((Get-Item $setup).Length / 1MB, 1)) MB)"
