# ================================
# STAGE 1 - Build
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia csproj e restaura dependências
COPY *.csproj ./
RUN dotnet restore

# Copia o restante do projeto
COPY . ./
RUN dotnet publish -c Release -o /out

# ================================
# STAGE 2 - Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Railway define PORT automaticamente
ENV ASPNETCORE_URLS=http://+:${PORT}

# Copia arquivos publicados
COPY --from=build /out .

# Informa a porta (Railway lê isso)
EXPOSE 8080

# Nome do DLL (AJUSTE se necessário)
ENTRYPOINT ["dotnet", "FilaDeCampo.dll"]
