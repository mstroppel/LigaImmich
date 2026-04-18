FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY FL.LigaImmich/FL.LigaImmich.csproj FL.LigaImmich/
COPY FL.LigaImmich.ImmichClient/FL.LigaImmich.ImmichClient.csproj FL.LigaImmich.ImmichClient/
RUN dotnet restore FL.LigaImmich/FL.LigaImmich.csproj

COPY . .
RUN dotnet publish FL.LigaImmich/FL.LigaImmich.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "FL.LigaImmich.dll"]
