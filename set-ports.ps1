$ErrorActionPreference = "Stop"

$ports = @{
  "Customer" = 5101
  "Identity" = 5102
  "Accounting" = 5103
  "Expense" = 5104
  "TransactionLimit" = 5105
  "Payments" = 5106
  "Integration" = 5107
  "Notification" = 5108
}

$apiBase = Join-Path "src" "services"

foreach ($domain in $ports.Keys) {
  $port = $ports[$domain]

  $launchPath = Join-Path $apiBase "$domain\$domain.API\Properties\launchSettings.json"
  if (-not (Test-Path $launchPath)) {
    Write-Host "❌ launchSettings not found: $launchPath" -ForegroundColor Red
    continue
  }

  $json = Get-Content $launchPath -Raw | ConvertFrom-Json

  # webapi template profile name is usually the project name; we just update all profiles
  foreach ($p in $json.profiles.PSObject.Properties.Name) {
    if ($json.profiles.$p.applicationUrl) {
      $json.profiles.$p.applicationUrl = "http://localhost:$port"
    } else {
      $json.profiles.$p | Add-Member -NotePropertyName "applicationUrl" -NotePropertyValue "http://localhost:$port" -Force
    }
  }

  $json | ConvertTo-Json -Depth 30 | Set-Content $launchPath -Encoding UTF8
  Write-Host "✅ $domain.API => http://localhost:$port" -ForegroundColor Green
}

Write-Host "`nDone. Restart Visual Studio profiles if open." -ForegroundColor Cyan
