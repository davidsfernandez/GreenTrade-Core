FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GreenTrade.Server/GreenTrade.Server.csproj", "GreenTrade.Server/"]
COPY ["GreenTrade.Client/GreenTrade.Client.csproj", "GreenTrade.Client/"]
COPY ["GreenTrade.Shared/GreenTrade.Shared.csproj", "GreenTrade.Shared/"]
RUN dotnet restore "./GreenTrade.Server/GreenTrade.Server.csproj"
COPY . .
WORKDIR "/src/GreenTrade.Server"
RUN dotnet build "./GreenTrade.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GreenTrade.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenTrade.Server.dll"]
