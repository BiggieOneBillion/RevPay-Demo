# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RevPay.API/RevPay.API.csproj", "src/RevPay.API/"]
COPY ["src/RevPay.Infrastructure/RevPay.Infrastructure.csproj", "src/RevPay.Infrastructure/"]
COPY ["src/RevPay.Application/RevPay.Application.csproj", "src/RevPay.Application/"]
COPY ["src/RevPay.Domain/RevPay.Domain.csproj", "src/RevPay.Domain/"]
RUN dotnet restore "./src/RevPay.API/RevPay.API.csproj"
COPY . .
WORKDIR "/src/src/RevPay.API"
RUN dotnet build "./RevPay.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RevPay.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RevPay.API.dll"]
