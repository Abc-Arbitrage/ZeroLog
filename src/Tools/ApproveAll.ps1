
$rootPath = Split-Path -Path $PSScriptRoot -Parent
$approved = 0;

Write-Host ""
Write-Host "Approving all received files in $rootPath" -ForegroundColor White
Write-Host ""

foreach ($file in Get-ChildItem -Path $rootPath -Recurse -Filter "*.received.*") {
    $approvedName = $file.FullName -replace "\.received\.(?=\w+$)", ".approved."
    Move-Item $file.FullName $approvedName -Force

    Write-Host "  ✅ $($file.Name)"
    $approved++
}

if ($approved -gt 0) {
    Write-Host ""
    Write-Host "Files approved: $approved" -ForegroundColor Green
}
else {
    Write-Host "No files approved" -ForegroundColor Yellow
}
