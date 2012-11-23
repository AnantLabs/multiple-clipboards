Param
(
    [switch]$useReleaseMode,
    [switch]$includeCustomActions,
    [switch]$buildBootstrapper
)

[System.IO.Directory]::SetCurrentDirectory(((Get-Location -PSProvider FileSystem).ProviderPath))

$wixSdkPath = "C:\Program Files (x86)\WiX Toolset v3.6\SDK\"
$installerRelativePath = ".\MultipleClipboards.Installer"
$wixInstallerFile = "Installer.wxs"
$wixBootstrapperFile = "Bootstrapper.wxs"
$wixIncludeFile = "Variables.wxi"
$installerObjectFileName = "MultipleClipboards.Installer.wixobj"
$bootstrapperObjectFileName = "MultipleClipbaords.Installer.Bootstrapper.wixobj"
$msiFileName = "MultipleClipboards.Installer.msi"

function WriteErrorAndExit ($message, $popLocation = $false)
{
    if ($popLocation)
    {
        Pop-Location
    }

    Write-Host $message -ForegroundColor Red
    Write-Host ""
    exit
}

function WriteProcessOutput ($output)
{
    if ($LASTEXITCODE -eq 0)
    {
        Write-Host $output
    }
    else
    {
        WriteErrorAndExit $output $true
    }
}

Write-Host ""

if (!(Test-Path "MultipleClipboards.sln"))
{
    WriteErrorAndExit "This script must be run from the root of a MultipleClipboards branch."
}

Write-Host "Cleaning old installer" -ForegroundColor Cyan
$installerObjOutput = [System.IO.Path]::Combine($installerRelativePath, "obj")
$installerBinOutput = [System.IO.Path]::Combine($installerRelativePath, "bin")

if (Test-Path $installerObjOutput)
{
    Get-ChildItem $installerObjOutput -Recurse | foreach ($_) { Remove-Item $_.FullName }
}
else
{
    New-Item $installerObjOutput -ItemType Directory | Out-Null
}

if (Test-Path $installerBinOutput)
{
    Get-ChildItem $installerBinOutput -Recurse | foreach ($_) { Remove-Item $_.FullName }
}
else
{
    New-Item $installerBinOutput -ItemType Directory | Out-Null
}

Push-Location
$mode = ""

if ($useReleaseMode)
{
    if (!(Test-Path .\MultipleClipboards\bin\Release))
    {
        WriteErrorAndExit "Release mode path does not exist.  Ensure that Multiple Clipboards has been built in release mode and try again."
    }

    $mode = "Release"
}
else
{
    if (!(Test-Path .\MultipleClipboards\bin\Debug))
    {
        WriteErrorAndExit "Debug mode path does not exist.  Ensure that Multiple Clipboards has been built in debug mode and try again."
    }

    $mode = "Debug"
}

$inputFolderAbsolutePath = [System.IO.Path]::GetFullPath($installerRelativePath)
$outputObjFolderAbsolutePath = [System.IO.Path]::GetFullPath($installerObjOutput)
$outputBinFolderAbsolutePath = [System.IO.Path]::GetFullPath($installerBinOutput)

Write-Host "Using the $mode mode build of Multiple Clipboards" -ForegroundColor Cyan

if ($includeCustomActions)
{
    Write-Host ""
    Write-Host "Creating Custom Action Package..." -ForegroundColor Cyan
    $makeSfxAbsolutePath = [System.IO.Path]::Combine($wixSdkPath, "MakeSfxCA.exe")
    $sfxcaDllAbsolutePath = [System.IO.Path]::Combine($wixSdkPath, "x64", "sfxca.dll")

    if ((Get-Command $makeSfxAbsolutePath) -and (Test-Path $sfxcaDllAbsolutePath))
    {
        $outputDllPath = [System.IO.Path]::Combine($outputObjFolderAbsolutePath, "MultipleClipboards.WixCustomActionsPackage.dll")
        $customActionDllAbsolutePath = [System.IO.Path]::GetFullPath(".\MultipleClipboards.WixCustomActions\bin\$mode\MultipleClipboards.WixCustomActions.dll")
        $globalResourcesDllAbsolutePath = [System.IO.Path]::GetFullPath(".\MultipleClipboards.WixCustomActions\bin\$mode\MultipleClipboards.GlobalResources.dll")
        $configAbsolutePath = [System.IO.Path]::GetFullPath(".\MultipleClipboards.WixCustomActions\CustomAction.config")
        $windowsDeploymentDllAbsolutePath = [System.IO.Path]::GetFullPath(".\References\Microsoft.Deployment.WindowsInstaller.dll")

        $output = (& "$makeSfxAbsolutePath" "$outputDllPath" "$sfxcaDllAbsolutePath" "$customActionDllAbsolutePath" "$configAbsolutePath" "$windowsDeploymentDllAbsolutePath" "$globalResourcesDllAbsolutePath") -join "`r`n"
        WriteProcessOutput $output
    }
    else
    {
        WriteErrorAndExit "Unable to find the WIX SDK.  Ensure WIX is installed and the correct path to the SDK folder is passed into this script." $true
    }
}

$wixInstallerFileAbsolutePath = [System.IO.Path]::Combine($inputFolderAbsolutePath, $wixInstallerFile)
$installerObjOutput = [System.IO.Path]::Combine($outputObjFolderAbsolutePath, $installerObjectFileName)
$installerBinOutput = [System.IO.Path]::Combine($outputBinFolderAbsolutePath, $msiFileName)
$wixBootstrapperFileAbsolutePath = [System.IO.Path]::Combine($inputFolderAbsolutePath, $wixBootstrapperFile)
$bootstrapperObjOutput = [System.IO.Path]::Combine($outputObjFolderAbsolutePath, $bootstrapperObjectFileName)
$bootstrapperBinOutput = [System.IO.Path]::Combine($outputBinFolderAbsolutePath, "setup.exe")

cd ".\MultipleClipboards\bin\$mode"
Write-Host ""

if (Get-Command "candle.exe")
{
    # Copy the WIX include file into the correct path
    $includeSource = [System.IO.Path]::Combine($inputFolderAbsolutePath, $wixIncludeFile)
    $licensePath = [System.IO.Path]::Combine($inputFolderAbsolutePath, 'license.rtf')
    Copy-Item $includeSource $wixIncludeFile
    Copy-Item $licensePath 'license.rtf'
    
    # Compile the insaller
    Write-Host "Compiling Installer..." -ForegroundColor Cyan
    $output = (candle.exe -o $installerObjOutput $wixInstallerFileAbsolutePath) -join "`r`n"
    WriteProcessOutput $output

    if ($buildBootstrapper)
    {
        # Compile the bootstrapper
        Write-Host "Compiling Bootstrapper..." -ForegroundColor Cyan
        $output = (candle.exe -ext WixBalExtension -o $bootstrapperObjOutput $wixBootstrapperFileAbsolutePath) -join "`r`n"
        WriteProcessOutput $output
    }
}
else
{
    WriteErrorAndExit "Unable to find 'candle.exe'.  Ensure WIX is installed and its bin directory has been added to the system path." $true
}

Write-Host ""

if (Get-Command "light.exe")
{
    # Link the installer
    Write-Host "Linking Installer..." -ForegroundColor Cyan
    $output = (light.exe -ext WixUIExtension -ext WixNetFxExtension -ext WixBalExtension -o $installerBinOutput $installerObjOutput) -join "`r`n"
    WriteProcessOutput $output

    if ($buildBootstrapper)
    {
        # Link the bootstrapper
        Pop-Location
        Push-Location
        cd "$installerRelativePath\bin"
        Write-Host "Linking Bootstrapper..." -ForegroundColor Cyan

        $output = (light.exe -ext WixNetFxExtension -ext WixBalExtension -o $bootstrapperBinOutput $bootstrapperObjOutput) -join "`r`n"
        WriteProcessOutput $output
    }
}
else
{
    WriteErrorAndExit "Unable to find 'light.exe'.  Ensure WIX is installed and its bin directory has been added to the system path." $true
}

Pop-Location
Write-Host "Done." -ForegroundColor Cyan
Write-Host ""
