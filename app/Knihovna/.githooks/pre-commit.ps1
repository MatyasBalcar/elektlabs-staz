Write-Host "Running Tests"
if ($env:SKIP_TESTS -or $env:NO_TEST) {
    Write-Host "SKIP_TESTS/NO_TEST environment variable detected; skipping tests." -ForegroundColor Yellow
    exit 0
}

Write-Host "Running Tests"
cd app/Knihovna/
dotnet test --nologo --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. Commit aborted." -ForegroundColor Red
    exit 1
}

Write-Host "Tests passed. Proceeding with commit." -ForegroundColor Green
exit 0
