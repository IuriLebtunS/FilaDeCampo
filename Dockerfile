# ================================
# BASE (runtime)
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER app

# ================================
# BUILD
# ================================
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["FilaDeCampo.csproj", "./"]
RUN dotnet restore "FilaDeCampo.csproj"

COPY . .
RUN dotnet build "FilaDeCampo.csproj" -c $configuration -o /app/build

# ================================
# PUBLISH
# ================================
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "FilaDeCampo.csproj" \
    -c $configuration \
    -o /app/publish \
    /p:UseAppHost=false

# ================================
# FINAL
# ================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FilaDeCampo.dll"]

