Write-Host "Running Tests"

dotnet test --nologo --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. Commit aborted." -ForegroundColor Red
    exit 1
}

Write-Host "Tests passed. Proceeding with commit." -ForegroundColor Green
exit 0