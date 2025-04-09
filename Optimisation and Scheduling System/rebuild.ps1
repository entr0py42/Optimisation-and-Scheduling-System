$projectPath = $PSScriptRoot
$binPath = Join-Path $projectPath "bin"
$objPath = Join-Path $projectPath "obj"

Write-Host "Cleaning project..."
if (Test-Path $binPath) {
    Remove-Item -Recurse -Force $binPath
}
if (Test-Path $objPath) {
    Remove-Item -Recurse -Force $objPath
}

Write-Host "Rebuilding project..."

# Not: Ara sıra yeni eklenen filelar update edilmiyordu o yüzden ekledim fyi dostlar
msbuild /t:Clean,Rebuild /p:Configuration=Debug /v:m

Write-Host "Done!" 