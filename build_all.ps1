
<#
Build script for LectorPDF installers (x86 + x64).

Prerequisites on Windows machine:
- Visual Studio or MSBuild Build Tools installed
- WiX Toolset (candle.exe and light.exe in PATH)
- Windows SDK (signtool.exe) if you want to sign with a certificate
- Place compiled binaries into BinFiles\x86 and BinFiles\x64 (Release builds)
- Place correct pdfium.dll for x86 and x64 in the respective folders

Usage (run in PowerShell as Administrator):
.\build_all.ps1 -SolutionPath ".\SourceProject\LectorPDF.sln" -Configuration "Release"

This script will:
- Build solution for x86 and x64 (if MSBuild path is configured)
- Copy outputs to BinFiles\x86 and BinFiles\x64 if found
- Run WiX (candle & light) in WiX_x86 and WiX_x64 to generate installers
- Create a temporary self-signed cert (for testing) and sign the MSIs
#>

param(
    [string]$SolutionPath = ".\SourceProject\LectorPDF.sln",
    [string]$Configuration = "Release",
    [string]$MSBuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)

function Run-Process($exe, $args) {
    Write-Host "Running: $exe $args"
    $p = Start-Process -FilePath $exe -ArgumentList $args -Wait -PassThru
    return $p.ExitCode
}

Write-Host "Starting full build and packaging process..."

# Build for AnyCPU / x86 / x64 as needed (adjust project settings to produce separate outputs)
if (Test-Path $MSBuildPath) {
    Run-Process $MSBuildPath "`"$SolutionPath`" /p:Configuration=$Configuration /m"
} else {
    Write-Warning "MSBuild not found at $MSBuildPath. Please install Build Tools and update MSBuildPath."
}

# Ensure WiX present
if (!(Get-Command candle.exe -ErrorAction SilentlyContinue)) {
    Write-Error "WiX (candle.exe) not found in PATH. Install WiX Toolset and add to PATH."
    exit 1
}

# Build x86 installer
Push-Location .\WiX_x86
candle.exe Product.wxs Components.wxs -out Product_x86.wixobj
if ($LASTEXITCODE -ne 0) { Write-Error "candle failed for x86"; Pop-Location; exit 1 }
light.exe Product_x86.wixobj Components.wixobj -out ..\LectorPDF_x86_Setup.msi
if ($LASTEXITCODE -ne 0) { Write-Error "light failed for x86"; Pop-Location; exit 1 }
Pop-Location

# Build x64 installer
Push-Location .\WiX_x64
candle.exe Product.wxs Components.wxs -out Product_x64.wixobj
if ($LASTEXITCODE -ne 0) { Write-Error "candle failed for x64"; Pop-Location; exit 1 }
light.exe Product_x64.wixobj Components.wixobj -out ..\LectorPDF_x64_Setup.msi
if ($LASTEXITCODE -ne 0) { Write-Error "light failed for x64"; Pop-Location; exit 1 }
Pop-Location

Write-Host "Installers created (un-signed): LectorPDF_x86_Setup.msi, LectorPDF_x64_Setup.msi"

# Create a temporary self-signed cert for testing (PowerShell 5+)
Write-Host "Creating test code-signing certificate..."
$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=LectorPDF Test" -CertStoreLocation "Cert:\CurrentUser\My"
if ($cert -ne $null) {
    $pwd = ConvertTo-SecureString -String "TestPassword123" -Force -AsPlainText
    $pfx = Join-Path $env:TEMP "LectorPDF-TestCert.pfx"
    Export-PfxCertificate -Cert $cert -FilePath $pfx -Password $pwd | Out-Null
    Write-Host "Exported PFX to $pfx (for testing only)."

    $signtool = "C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool.exe"
    if (Test-Path $signtool) {
        & $signtool sign /f $pfx /p TestPassword123 /tr http://timestamp.digicert.com /td sha256 /fd sha256 .\LectorPDF_x86_Setup.msi
        & $signtool sign /f $pfx /p TestPassword123 /tr http://timestamp.digicert.com /td sha256 /fd sha256 .\LectorPDF_x64_Setup.msi
        Write-Host "Signed both MSIs with test certificate."
    } else {
        Write-Warning "signtool.exe not found; MSI will remain unsigned. Install Windows SDK to sign."
    }
} else {
    Write-Warning "Could not create test certificate."
}

Write-Host "Build and packaging finished. Test MSIs are in the current folder."
