FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Repo root context'ten gerekli her şeyi kopyala
COPY . .

# API projesini publish et (path önemli!)
RUN dotnet restore "src/services/Identity/Identity.API/GaniPay.Identity.API.csproj"
RUN dotnet publish "src/services/Identity/Identity.API/GaniPay.Identity.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GaniPay.Identity.API.dll"]
