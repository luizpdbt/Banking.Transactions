
# Etapa 1 - Imagem base de execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

# Etapa 2 - Compilação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copia o projeto para restaurar as dependências
COPY ["Banking.Transactions/Banking.Barramento.Api.csproj", "Banking.Transactions/"]

RUN dotnet restore "Banking.Transactions/Banking.Barramento.Api.csproj"

# Copia os demais arquivos do repositório
COPY . .

WORKDIR "/src/Banking.Transactions"

RUN dotnet build "Banking.Barramento.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# Etapa 3 - Publicação
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Banking.Barramento.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 4 - Imagem final
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Banking.Barramento.Api.dll"]
