Param
(
    [boolean]$useReleaseMode = $false,
    [boolean]$includeCustomActions = $false,
    [string]$wixSdkPath = "C:\Program Files (x86)\WiX Toolset v3.6\SDK\",
    [string]$installerRelativePath = ".\MultipleClipboards.Installer",
    [string]$inputWxsFile = "Installer.wxs",
    [string]$objectFileName = "MultipleClipboards.Installer.wixobj",
    [string]$msiFileName = "MultipleClipboards.Installer.msi"
)

[System.IO.Directory]::SetCurrentDirectory(((Get-Location -PSProvider FileSystem).ProviderPath))

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
$outputObjPath = [System.IO.Path]::Combine($installerRelativePath, "obj")
$outputBinPath = [System.IO.Path]::Combine($installerRelativePath, "bin")

if (Test-Path $outputObjPath)
{
    Get-ChildItem $outputObjPath -Recurse | foreach ($_) { Remove-Item $_.FullName }
}
else
{
    New-Item $outputObjPath -ItemType Directory | Out-Null
}

if (Test-Path $outputBinPath)
{
    Get-ChildItem $outputBinPath -Recurse | foreach ($_) { Remove-Item $_.FullName }
}
else
{
    New-Item $outputBinPath -ItemType Directory | Out-Null
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
$outputObjFolderAbsolutePath = [System.IO.Path]::GetFullPath($outputObjPath)
$outputBinFolderAbsolutePath = [System.IO.Path]::GetFullPath($outputBinPath)

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

$inputFilePath = [System.IO.Path]::Combine($inputFolderAbsolutePath, $inputWxsFile)
$outputObjPath = [System.IO.Path]::Combine($outputObjFolderAbsolutePath, $objectFileName)
$outputBinPath = [System.IO.Path]::Combine($outputBinFolderAbsolutePath, $msiFileName)

cd ".\MultipleClipboards\bin\$mode"
Write-Host ""
Write-Host "Compiling Installer..." -ForegroundColor Cyan

if (Get-Command "candle.exe")
{
    $output = (candle.exe -o $outputObjPath $inputFilePath) -join "`r`n"
    WriteProcessOutput $output
}
else
{
    WriteErrorAndExit "Unable to find 'candle.exe'.  Ensure WIX is installed and its bin directory has been added to the system path." $true
}

Write-Host ""
Write-Host "Linking Installer..." -ForegroundColor Cyan

if (Get-Command "light.exe")
{
    $output = (light.exe -ext WixUIExtension -ext WixNetFxExtension -o $outputBinPath $outputObjPath) -join "`r`n"
    WriteProcessOutput $output
}
else
{
    WriteErrorAndExit "Unable to find 'light.exe'.  Ensure WIX is installed and its bin directory has been added to the system path." $true
}

Pop-Location
Write-Host "Done." -ForegroundColor Cyan
Write-Host ""
