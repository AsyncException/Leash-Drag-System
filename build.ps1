#This value is used for the name of the shortcuts and the appdata folder name.
$humanFriendlyName = "Leash Drag System"

#Name of the project folder and csproj
$appName = "LDS"

#Publisher name
$appPublisher = "Async"

$versionPrefixMatch = "<VersionPrefix>(.*?)<\/VersionPrefix>"
$versionSuffixMatch = "<VersionSuffix>(.*?)<\/VersionSuffix>"

$setupOutput = "\builds"
$innoPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$innoConfig = "installer\Inno Setup Script.iss"


#autoprops
$projectPath = "src\$appName\$appName.csproj"
$appSourcePath = "\builds\$appName\*"
$appExecutable = "$appName.exe"

$basePath = Split-Path -Parent $PSCommandPath

try {
    $projectContent = Get-Content -Path "$basePath\$projectPath"
} catch {
    Write-Error "Cant find app .csproj file at $("$basePath\$projectPath")"
    return
}

try {
    $appVersion = [Regex]::Match($projectContent, $versionPrefixMatch).Captures.Groups[1].Value;
} catch {
    Write-Error "Cannot find <VersionPrefix></VersionPrefix> in .csproj file"
    return
}

try {
    $versionSuffix = [Regex]::Match($projectContent, $versionSuffixMatch).Captures.Groups[1].Value
    if($versionSuffix){
        $appVersion += "-$versionSuffix";
    }
} catch {}


Write-Host "Starting dotnet publish"
dotnet publish "$basePath\$projectPath" `
    --configuration Release `
    --runtime win-x64 `
    /p:PublishProfile= `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    /p:SelfContained=true `
    -v:normal

Write-Host "Removing language folders"

Get-ChildItem $basePath -Recurse -Filter "Microsoft.ui.xaml.dll.mui" | Select-Object -ExpandProperty Directory -unique | Where-Object name -ne "en-us" | ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -Confirm:$false }

$confirmation = Read-Host "Do you want to build the installer?"
if($confirmation -eq "Y" -or $confirmation -eq "y") {
    Write-Host "Building installer"
    & "$innoPath" "/DBasePath=$basePath" "/DVersion=$appVersion" "/DAppName=$humanFriendlyName" "/DAppPublisher=$appPublisher" "/DAppExeName=$appExecutable" "/DAppSourcePath=$appSourcePath" "/DSetupOutput=$setupOutput" "$basePath\$innoConfig"
}