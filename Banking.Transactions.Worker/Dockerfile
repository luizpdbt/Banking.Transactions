
# Etapa 1 - Imagem base de execução
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

USER $APP_UID
WORKDIR /app

# Etapa 2 - Compilação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copia o arquivo do projeto
COPY ["Banking.Transactions.Worker/Banking.Transactions.Worker.csproj", "Banking.Transactions.Worker/"]

# Restaura as dependências
RUN dotnet restore "Banking.Transactions.Worker/Banking.Transactions.Worker.csproj"

# Copia os demais arquivos do repositório
COPY . .

WORKDIR "/src/Banking.Transactions.Worker"

# Compila a aplicação
RUN dotnet build "Banking.Transactions.Worker.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# Etapa 3 - Publicação
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Banking.Transactions.Worker.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 4 - Imagem final
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Banking.Transactions.Worker.dll"]
