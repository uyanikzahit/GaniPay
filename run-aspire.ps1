$env:ASPIRE_ALLOW_UNSECURED_TRANSPORT = "true"
$env:DOTNET_DASHBOARD_OTLP_ENDPOINT_URL = "https://127.0.0.1:0"
Remove-Item Env:DOTNET_DASHBOARD_OTLP_HTTP_ENDPOINT_URL -ErrorAction SilentlyContinue

dotnet run --project .\GaniPay.AppHost\GaniPay.AppHost.csproj
