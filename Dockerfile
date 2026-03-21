# ── Stage 1: build & publish ─────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first — isolates restore layer for better cache reuse.
# Only the projects in the dependency chain of the gateway are needed.
COPY ["MyCRM.sln", "."]
COPY ["1 - Gateway/MundoDaLua.GraphQL/MyCRM.GraphQL.csproj",           "1 - Gateway/MundoDaLua.GraphQL/"]
COPY ["2 - CRM/CRM.Domain/MyCRM.CRM.Domain.csproj",                    "2 - CRM/CRM.Domain/"]
COPY ["2 - CRM/CRM.Application/MyCRM.CRM.Application.csproj",          "2 - CRM/CRM.Application/"]
COPY ["2 - CRM/CRM.Infrastructure/MyCRM.CRM.Infrastructure.csproj",    "2 - CRM/CRM.Infrastructure/"]
COPY ["3 - Auth/Auth.Domain/MyCRM.Auth.Domain.csproj",                  "3 - Auth/Auth.Domain/"]
COPY ["3 - Auth/Auth.Application/MyCRM.Auth.Application.csproj",        "3 - Auth/Auth.Application/"]
COPY ["3 - Auth/Auth.Infrastructure/MyCRM.Auth.Infrastructure.csproj",  "3 - Auth/Auth.Infrastructure/"]
COPY ["3 - Shared/Shared.Kernel/MyCRM.Shared.Kernel.csproj",            "3 - Shared/Shared.Kernel/"]

RUN dotnet restore "1 - Gateway/MundoDaLua.GraphQL/MyCRM.GraphQL.csproj"

# Copy full source after restore — changes here don't bust the restore cache
COPY . .

RUN dotnet publish "1 - Gateway/MundoDaLua.GraphQL/MyCRM.GraphQL.csproj" \
        --configuration Release \
        --output /app/publish \
        --no-restore \
        -p:UseAppHost=false

# ── Stage 2: runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user — principle of least privilege
RUN addgroup --system --gid 1001 appgroup \
 && adduser  --system --uid 1001 --ingroup appgroup --no-create-home appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

# ASP.NET Core listens on 8080 in non-root context (port 80 requires root)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

ENTRYPOINT ["dotnet", "MyCRM.GraphQL.dll"]
