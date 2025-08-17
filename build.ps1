$basePath = Split-Path -Parent $PSCommandPath
$projectPath = "src\LDS\LDS.csproj"
$versionPrefixMatch = "<VersionPrefix>(.*?)<\/VersionPrefix>"
$versionSuffixMatch = "<VersionSuffix>(.*?)<\/VersionSuffix>"

$projectContent = Get-Content -Path "$basePath\$projectPath"
$versionPrefix = [Regex]::Match($projectContent, $versionPrefixMatch).Captures.Groups[1].Value;
$versionSuffix = [Regex]::Match($projectContent, $versionSuffixMatch).Captures.Groups[1].Value

Write-Host "Starting dotnet publish"
dotnet publish "$basePath\$projectPath" `
    --configuration Release `
    --runtime win-x64 `
    /p:PublishProfile= `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    /p:SelfContained=true `
    -v:normal

$confirmation = Read-Host "Do you want to build the installer?"
if($confirmation -eq "Y" -or $confirmation -eq "y"){
    Write-Host "Building installer"
    $innoPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    $innoConfig = ".\installer\Inno Setup Script.iss"

    & "$innoPath" "/DBasePath=$basePath" "/DVersion=$versionPrefix-$versionSuffix" $innoConfig
}

