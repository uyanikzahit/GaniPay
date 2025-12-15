$ErrorActionPreference = "Stop"

# =============================
# Config
# =============================
$SolutionName = "GaniPay"
$Src = "src"
$Services = Join-Path $Src "services"
$Blocks = Join-Path $Src "building-blocks"
$Workflow = Join-Path $Src "workflow"

$domains = @(
  "Customer",
  "Identity",
  "Accounting",
  "Payments",
  "Expense",
  "TransactionLimit",
  "Integration",
  "Notification"
)

function Ensure-Dir($path) {
  if (-not (Test-Path $path)) { New-Item -ItemType Directory -Path $path | Out-Null }
}

# =============================
# Create folders
# =============================
Ensure-Dir $Src
Ensure-Dir $Services
Ensure-Dir $Blocks
Ensure-Dir $Workflow

Ensure-Dir (Join-Path $Blocks "GaniPay.Common")
Ensure-Dir (Join-Path $Blocks "GaniPay.SharedKernel")

# =============================
# Create solution
# =============================
dotnet new sln -n $SolutionName

# =============================
# Building Blocks Projects
# =============================
dotnet new classlib -n "GaniPay.Common" -o (Join-Path $Blocks "GaniPay.Common")
dotnet new classlib -n "GaniPay.SharedKernel" -o (Join-Path $Blocks "GaniPay.SharedKernel")

dotnet sln add (Join-Path $Blocks "GaniPay.Common\GaniPay.Common.csproj")
dotnet sln add (Join-Path $Blocks "GaniPay.SharedKernel\GaniPay.SharedKernel.csproj")

# =============================
# Domain Services Projects
# =============================
foreach ($d in $domains) {
  $base = Join-Path $Services $d
  Ensure-Dir $base

  $api = Join-Path $base "$d.API"
  $app = Join-Path $base "$d.Application"
  $dom = Join-Path $base "$d.Domain"
  $inf = Join-Path $base "$d.Infrastructure"

  dotnet new webapi  -n "GaniPay.$d.API"           -o $api --no-https
  dotnet new classlib -n "GaniPay.$d.Application"  -o $app
  dotnet new classlib -n "GaniPay.$d.Domain"       -o $dom
  dotnet new classlib -n "GaniPay.$d.Infrastructure" -o $inf

  dotnet sln add (Join-Path $api "GaniPay.$d.API.csproj")
  dotnet sln add (Join-Path $app "GaniPay.$d.Application.csproj")
  dotnet sln add (Join-Path $dom "GaniPay.$d.Domain.csproj")
  dotnet sln add (Join-Path $inf "GaniPay.$d.Infrastructure.csproj")

  # References: API -> Application, Infrastructure
  dotnet add (Join-Path $api "GaniPay.$d.API.csproj") reference (Join-Path $app "GaniPay.$d.Application.csproj")
  dotnet add (Join-Path $api "GaniPay.$d.API.csproj") reference (Join-Path $inf "GaniPay.$d.Infrastructure.csproj")

  # Infrastructure -> Application, Domain, SharedKernel, Common
  dotnet add (Join-Path $inf "GaniPay.$d.Infrastructure.csproj") reference (Join-Path $app "GaniPay.$d.Application.csproj")
  dotnet add (Join-Path $inf "GaniPay.$d.Infrastructure.csproj") reference (Join-Path $dom "GaniPay.$d.Domain.csproj")
  dotnet add (Join-Path $inf "GaniPay.$d.Infrastructure.csproj") reference (Join-Path $Blocks "GaniPay.SharedKernel\GaniPay.SharedKernel.csproj")
  dotnet add (Join-Path $inf "GaniPay.$d.Infrastructure.csproj") reference (Join-Path $Blocks "GaniPay.Common\GaniPay.Common.csproj")

  # Application -> Domain, SharedKernel, Common
  dotnet add (Join-Path $app "GaniPay.$d.Application.csproj") reference (Join-Path $dom "GaniPay.$d.Domain.csproj")
  dotnet add (Join-Path $app "GaniPay.$d.Application.csproj") reference (Join-Path $Blocks "GaniPay.SharedKernel\GaniPay.SharedKernel.csproj")
  dotnet add (Join-Path $app "GaniPay.$d.Application.csproj") reference (Join-Path $Blocks "GaniPay.Common\GaniPay.Common.csproj")

  # Domain -> SharedKernel
  dotnet add (Join-Path $dom "GaniPay.$d.Domain.csproj") reference (Join-Path $Blocks "GaniPay.SharedKernel\GaniPay.SharedKernel.csproj")
}

# =============================
# Workflow Worker (single host)
# =============================
$workerDir = Join-Path $Workflow "Workflow.PaymentProcessWorker"
Ensure-Dir $workerDir

dotnet new console -n "GaniPay.Workflow.PaymentProcessWorker" -o $workerDir
dotnet sln add (Join-Path $workerDir "GaniPay.Workflow.PaymentProcessWorker.csproj")

# Worker references (Application layers it will call)
foreach ($d in $domains) {
  $appPath = Join-Path $Services "$d\$d.Application\GaniPay.$d.Application.csproj"
  if (Test-Path $appPath) {
    dotnet add (Join-Path $workerDir "GaniPay.Workflow.PaymentProcessWorker.csproj") reference $appPath
  }
}

Write-Host "✅ Bootstrap completed. Now run: dotnet build" -ForegroundColor Green
