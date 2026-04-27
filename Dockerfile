# ============================================================
# Stage 1: Restore (layer-cached on .csproj changes only)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

COPY backend/FitPlan.sln ./
COPY backend/src/FitPlan.Api/FitPlan.Api.csproj                           src/FitPlan.Api/
COPY backend/src/FitPlan.Application/FitPlan.Application.csproj           src/FitPlan.Application/
COPY backend/src/FitPlan.Domain/FitPlan.Domain.csproj                     src/FitPlan.Domain/
COPY backend/src/FitPlan.Infrastructure/FitPlan.Infrastructure.csproj     src/FitPlan.Infrastructure/
COPY backend/tests/FitPlan.Domain.Tests/FitPlan.Domain.Tests.csproj                   tests/FitPlan.Domain.Tests/
COPY backend/tests/FitPlan.Application.Tests/FitPlan.Application.Tests.csproj         tests/FitPlan.Application.Tests/
COPY backend/tests/FitPlan.Api.IntegrationTests/FitPlan.Api.IntegrationTests.csproj   tests/FitPlan.Api.IntegrationTests/

RUN dotnet restore

# ============================================================
# Stage 2: Build
# ============================================================
FROM restore AS build
WORKDIR /src

COPY backend/ ./

RUN dotnet build -c Release --no-restore

# ============================================================
# Stage 3: Publish
# ============================================================
FROM build AS publish

RUN dotnet publish src/FitPlan.Api/FitPlan.Api.csproj -c Release --no-build -o /app/publish

# ============================================================
# Stage 4: Runtime
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

RUN groupadd --system --gid 1001 appgroup \
 && useradd --system --uid 1001 --gid appgroup appuser \
 && apt-get update && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

COPY --from=publish --chown=appuser:appgroup /app/publish ./

USER appuser

EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "FitPlan.Api.dll"]
