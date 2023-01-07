FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DaprAsbEmulator/DaprAsbEmulator.csproj", "DaprAsbEmulator/"]
RUN dotnet restore "DaprAsbEmulator/DaprAsbEmulator.csproj"
COPY . .
WORKDIR "/src/DaprAsbEmulator"
RUN dotnet build "DaprAsbEmulator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DaprAsbEmulator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DaprAsbEmulator.dll"]
