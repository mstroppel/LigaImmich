FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln global.json ./
COPY FL.LigaImmich/FL.LigaImmich.csproj FL.LigaImmich/
COPY FL.LigaImmich.ImmichClient/FL.LigaImmich.ImmichClient.csproj FL.LigaImmich.ImmichClient/
COPY FL.LigaImmich.ImmichClient.Generation/FL.LigaImmich.ImmichClient.Generation.csproj FL.LigaImmich.ImmichClient.Generation/
RUN dotnet restore

COPY . .
RUN dotnet publish FL.LigaImmich/FL.LigaImmich.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "FL.LigaImmich.dll"]
