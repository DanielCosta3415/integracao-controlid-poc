# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
WORKDIR /src

COPY Directory.Build.props global.json ./
COPY Integracao.ControlID.PoC.csproj packages.lock.json ./
RUN dotnet restore ./Integracao.ControlID.PoC.csproj --locked-mode

COPY . .
RUN dotnet publish ./Integracao.ControlID.PoC.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false \
    /p:ContinuousIntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/integracao_controlid.db"

RUN if ! grep -q '^app:' /etc/group; then addgroup -S app; fi && \
    if ! id -u app >/dev/null 2>&1; then adduser -S -G app app; fi && \
    mkdir -p /app/data /app/Logs && \
    chown -R app:app /app

COPY --from=build --chown=app:app /app/publish ./

USER app
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Integracao.ControlID.PoC.dll"]
