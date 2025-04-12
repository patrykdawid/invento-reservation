# run-tests.ps1

$ErrorActionPreference = "Stop"
$startTime = Get-Date
$solutionName = Split-Path -Leaf (Get-Location)

# Correct directory structure for your solution
$testsDir = "tests"
$reportDir = Join-Path $testsDir "coverage-report"
$resultsDir = Join-Path $testsDir "TestResults"
$settingsPath = "tests/coverlet.runsettings"

Write-Host ""
Write-Host "=== Running Tests for Project: $solutionName ===" -ForegroundColor Cyan
Write-Host "Start Time: $startTime" -ForegroundColor DarkGray
Write-Host ""

try {
    # Step 1: Clean previous results
    if (Test-Path $reportDir) {
        Remove-Item $reportDir -Recurse -Force
        Write-Host "Removed old coverage-report directory" -ForegroundColor DarkGray
    }

    if (Test-Path $resultsDir) {
        Remove-Item $resultsDir -Recurse -Force
        Write-Host "Removed old TestResults directory" -ForegroundColor DarkGray
    }

    # Step 2: Clean + build
    Write-Host "`nCleaning and building..." -ForegroundColor Yellow
    dotnet clean
    dotnet build

    # Step 3: Run tests with coverage
    Write-Host "`nRunning tests with coverage..." -ForegroundColor Yellow
    dotnet test tests/backend.Tests/backend.Tests.csproj `
        --results-directory "$resultsDir" `
        --collect:"XPlat Code Coverage" `
        --settings $settingsPath

    # Step 4: Find coverage file
    $coverageFile = Get-ChildItem -Recurse -Path $resultsDir -Filter "coverage.cobertura.xml" | Select-Object -First 1
    if (-not $coverageFile) {
        Write-Host "`n[ERROR] coverage.cobertura.xml not found." -ForegroundColor Red
        throw "Coverage file not found."
    }

    # Step 5: Generate HTML report
    Write-Host "`nGenerating HTML report..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool --no-cache -v q
    reportgenerator -reports:$coverageFile.FullName -targetdir:$reportDir -reporttypes:Html

    # Step 6: Open report
    $reportIndex = Join-Path $reportDir "index.html"
    if (Test-Path $reportIndex) {
        Write-Host "`nReport successfully generated: $reportIndex" -ForegroundColor Green
        Start-Process $reportIndex
    } else {
        Write-Host "`n[ERROR] Report index.html not found." -ForegroundColor Red
    }

} catch {
    Write-Host "`n[EXCEPTION] $($_.Exception.Message)" -ForegroundColor Red
}

# Final info
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`nEnd Time: $endTime" -ForegroundColor DarkGray
Write-Host "Duration : $($duration.ToString("hh\:mm\:ss"))" -ForegroundColor DarkGray

Write-Host "`n=== Process complete. Press Enter to exit. ===" -ForegroundColor Cyan
Read-Host
