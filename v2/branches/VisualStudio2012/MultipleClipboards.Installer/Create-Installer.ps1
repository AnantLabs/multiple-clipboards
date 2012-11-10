Param
(
    [boolean]$useReleaseMode = $false,
    [string]$installerRelativePath = ".\MultipleClipboards.Installer",
    [string]$inputWxsFile = "Installer.wxs",
    [string]$objectFileName = "MultipleClipboards.Installer.wixobj",
    [string]$msiFileName = "MultipleClipboards.Installer.msi"
)

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

Write-Host ""

if (!(Test-Path "MultipleClipboards.sln"))
{
    WriteErrorAndExit "This script must be run from the root of a MultipleClipboards branch."
}

Write-Host "Cleaning old installer" -ForegroundColor Cyan
$outputObjPath = $installerRelativePath + "\obj"
$outputBinPath = $installerRelativePath + "\bin"

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

    $mode = "release"
    cd .\MultipleClipboards\bin\Release
}
else
{
    if (!(Test-Path .\MultipleClipboards\bin\Debug))
    {
        WriteErrorAndExit "Debug mode path does not exist.  Ensure that Multiple Clipboards has been built in debug mode and try again."
    }

    $mode = "debug"
    cd .\MultipleClipboards\bin\Debug
}

$inputFilePath = "..\..\." + $installerRelativePath + "\" + $inputWxsFile
$outputObjPath = "..\..\." + $outputObjPath + "\" + $objectFileName
$outputBinPath = "..\..\." + $outputBinPath + "\" + $msiFileName

Write-Host "Using the $mode mode build of Multiple Clipboards" -ForegroundColor Cyan
Write-Host "Compiling Installer..." -ForegroundColor Cyan

if (Get-Command "candle.exe")
{
    candle.exe -o $outputObjPath $inputFilePath
}
else
{
    WriteErrorAndExit "Unable to find 'candle.exe'.  Ensure WIX is installed and its bin directory has been added to the system path." $true
}

Write-Host "Linking Installer..." -ForegroundColor Cyan

if (Get-Command "light.exe")
{
    light.exe -ext WixUIExtension -ext WixNetFxExtension -o $outputBinPath $outputObjPath
}
else
{
    WriteErrorAndExit "Unable to find 'light.exe'.  Ensure WIX is installed and its bin directory has been added to the system path." $true
}

Pop-Location
Write-Host "Done." -ForegroundColor Cyan
Write-Host ""
