FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /src
COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Web/Web.csproj", "src/Web/"]
RUN dotnet restore "src/Web/Web.csproj"
RUN dotnet tool install --global dotnet-ef --version "10.*"

COPY . .
RUN mkdir -p /artifacts/publish /artifacts
RUN dotnet publish "src/Web/Web.csproj" -c "$BUILD_CONFIGURATION" -o /artifacts/publish /p:UseAppHost=false
RUN dotnet ef migrations bundle --project "src/Infrastructure/Infrastructure.csproj" --startup-project "src/Web/Web.csproj" --configuration "$BUILD_CONFIGURATION" --output /artifacts/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && useradd --create-home --home-dir /home/acadsign --shell /usr/sbin/nologin --uid 10001 acadsign

WORKDIR /app
COPY --from=build /artifacts/publish ./
COPY --from=build /artifacts/efbundle /app/efbundle
RUN chown -R acadsign:acadsign /app \
    && chmod +x /app/efbundle

USER acadsign
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 CMD curl --fail http://127.0.0.1:8080/health || exit 1
ENTRYPOINT ["dotnet", "AcadSign.Backend.Web.dll"]
