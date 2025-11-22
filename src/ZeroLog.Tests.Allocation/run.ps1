#!/usr/bin/env pwsh
$ErrorActionPreference = 'Stop'

$targetFrameworks = (dotnet msbuild -getProperty:TargetFrameworks "$PSScriptRoot") -split ';' |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ -ne '' } |
        Sort-Object { [Version]($_ -replace 'net', '') }

if ($targetFrameworks.Count -eq 0) {
    Write-Host -ForegroundColor Red 'No target frameworks found.'
    exit 1
}

Write-Host "Target frameworks: $( $targetFrameworks -join ', ' )"
$failedFrameworks = @()

foreach ($fwk in $targetFrameworks) {
    dotnet run --framework $fwk --project "$PSScriptRoot" $args
    if ($LASTEXITCODE -ne 0) {
        $failedFrameworks += $fwk
    }
}

if ($failedFrameworks.Count -gt 0) {
    Write-Host -ForegroundColor Red "Allocation tests failed: $( $failedFrameworks -join ', ' )"
    exit 1
}

Write-Host -ForegroundColor Green 'All allocation tests passed.'
exit 0
