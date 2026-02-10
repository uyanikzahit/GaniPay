# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Cache restore
COPY ["src/services/Payments/Payments.API/GaniPay.Payments.API.csproj", "src/services/Payments/Payments.API/"]
COPY ["src/services/Payments/Payments.Application/GaniPay.Payments.Application.csproj", "src/services/Payments/Payments.Application/"]
COPY ["src/services/Payments/Payments.Domain/GaniPay.Payments.Domain.csproj", "src/services/Payments/Payments.Domain/"]
COPY ["src/services/Payments/Payments.Infrastructure/GaniPay.Payments.Infrastructure.csproj", "src/services/Payments/Payments.Infrastructure/"]

RUN dotnet restore "src/services/Payments/Payments.API/GaniPay.Payments.API.csproj"

# Copy everything and publish
COPY . .
WORKDIR "/src/src/services/Payments/Payments.API"
RUN dotnet publish "GaniPay.Payments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GaniPay.Payments.API.dll"]
