param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [String[]]$Args
)

# Wrapper to allow using --no-test which sets SKIP_TESTS before invoking git commit
$skipTests = $false
$filteredArgs = @()

foreach ($arg in $Args) {
    if ($arg -eq '--no-test') {
        $skipTests = $true
        continue
    }
    $filteredArgs += $arg
}

if ($skipTests) {
    # Set environment variable for this process so hooks see it
    $env:SKIP_TESTS = '1'
}

# Call git commit with the filtered arguments
git commit @filteredArgs

# Unset environment variable to avoid leaking
if ($skipTests) { Remove-Item env:SKIP_TESTS }
