$basePath = Split-Path -Parent $PSCommandPath

Write-Host "Starting dotnet publish"
dotnet publish "$basePath\src\LDS\LDS.csproj" `
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

    & "$innoPath" "/DBasePath=$basePath" $innoConfig
}

