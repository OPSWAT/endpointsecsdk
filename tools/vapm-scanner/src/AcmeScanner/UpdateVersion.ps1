param (
    [string]$csprojFile
)

if (-not (Test-Path $csprojFile)) {
    Write-Host "Project file not found: $csprojFile"
    exit 1
}

[xml]$xml = Get-Content $csprojFile

$versionNode = $xml.Project.PropertyGroup.Version

$versionParts = $versionNode -split '\.'

$versionParts[3] = [int]$versionParts[3] + 1

$newVersion = $versionParts -join '.'

$xml.Project.PropertyGroup.Version = $newVersion

$xml.Save($csprojFile)

